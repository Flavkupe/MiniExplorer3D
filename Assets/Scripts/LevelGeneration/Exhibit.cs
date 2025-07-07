using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public enum ExhibitListItemMode
{
    /// <summary>
    /// List items are skipped outright
    /// </summary>
    Skip = 0,

    /// <summary>
    /// List items are combined with paragraphs, with each item treated as a paragraph.
    /// </summary>
    CombineWithParagraphs = 1,

    /// <summary>
    /// Represents a combined state where multiple elements are merged into a single paragraph.
    /// </summary>
    CombinedIntoOneParagraph = 2,

    // TODO: eventually, add actual specific displays for list items
}

public class Exhibit : ExhibitBase
{
    // Arbitrary limit for reading content
    const int SkipContentLimit = 8;

    private SectionData section;

    [Tooltip("How list items are displayed.")]
    public ExhibitListItemMode ListItemMode = ExhibitListItemMode.Skip;

    private RoomImageFrame[] Paintings = new RoomImageFrame[] { };
    private Placeholder[] Reading = new Placeholder[] { };
    private Placeholder[] DisplayPodiums = new Placeholder[] { };
    private Placeholder TOCPodium = null;
    private AreaTitle AreaTitleSign = null;
    private List<Door> Exits = new();

    public ExhibitBase[] SubExhibits { get; private set; } = new ExhibitBase[] { };

    private bool isPopulated = false;

    public override void PopulateParts()
    {
        if (isPopulated)
        {
            return;
        }

        // Only get immediate children for Placeholders, RoomImageFrame, and SubExhibits
        var children = new List<GameObject>();
        for (int i = 0; i < transform.childCount; ++i)
            children.Add(transform.GetChild(i).gameObject);
        this.Paintings = children.SelectMany(go => go.GetComponents<RoomImageFrame>()).Where(a => a.FrameType == RoomImageFrame.ImageFrameType.Painting).ToArray();
        var allPlaceholders = children.SelectMany(go => go.GetComponents<Placeholder>()).ToList();
        this.Reading = allPlaceholders.Where(a => a.PartType == Placeholder.RoomPartType.Reading).ToArray();
        this.DisplayPodiums = allPlaceholders.Where(a => a.PartType == Placeholder.RoomPartType.DisplayPodium ||
                                                         a.PartType == Placeholder.RoomPartType.TextPodium).ToArray();
        this.TOCPodium = allPlaceholders.FirstOrDefault(a => a.PartType == Placeholder.RoomPartType.TableOfContentsPodium);
        this.AreaTitleSign = children.SelectMany(go => go.GetComponents<AreaTitle>()).FirstOrDefault();
        this.SubExhibits = children.SelectMany(go => go.GetComponents<ExhibitBase>()).Where(e => e != this).ToArray();
        this.Exits = children.SelectMany(go => go.GetComponents<Door>()).ToList();
        foreach (var exhibit in this.SubExhibits)
        {
            exhibit.PopulateParts();
        }
    }

    public override bool CanHandleSection(SectionData section)
    {
        PopulateParts();

        if (section == null)
        {
            return false;
        }

        // SectionType flag check
        if ((SectionTypes & section.SectionType) == 0)
        {
            return false;
        }

        // Rule 1: AreaTitleSign iff Title is non-empty
        bool hasTitle = !string.IsNullOrEmpty(section.Title);
        if (hasTitle && !this.SupportsTitle())
        {
            return false;
        }

        //// Rule 2: Enough Reading items for LocationText
        //int textCount = section.LocationText.Count;
        //int readingCount = this.GetReadingCount();
        //if (readingCount < textCount)
        //{
        //    if (textCount > SkipContentLimit)
        //    {
        //        Debug.LogWarning($"Exhibit {this.PrefabID} has too many reading items ({textCount}) for the available placeholders ({readingCount}). Skipping some content.");
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        // Rule 3: Subsections must be handled by subexhibits
        if (section.Subsections.Count > 0)
        {
            if (SubExhibits == null || SubExhibits.Length == 0)
            {
                return false;
            }

            // Each subsection must be handled by at least one subexhibit
            foreach (var subSection in section.Subsections)
            {
                bool handled = SubExhibits.Any(sub => sub.CanHandleSection(subSection));
                if (!handled)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public override void ClearAssignment()
    {
        section = null;
        IsAssigned = false;
        if (SubExhibits != null)
        {
            foreach (var sub in SubExhibits)
            {
                sub.ClearAssignment();
            }
        }
    }

    // Populate the exhibit's parts (text, images, etc.) from the SectionData
    public override void PopulateExhibit(ExhibitData data)
    {
        PopulateParts();
        this.section = data?.SectionData;
        if (this.section == null)
        {
            this.gameObject.SetActive(false);
            return;
        }
        IsAssigned = true;

        // Handle list items according to the mode
        HandleListItems();

        // Example: Set AreaTitle if present
        if (this.AreaTitleSign != null && !string.IsNullOrEmpty(section.Title))
        {
            this.AreaTitleSign.SetTitle(section.Title);
        }

        // Place the images
        foreach (RoomImageFrame frame in this.Paintings)
        {
            if (section.ImagePaths != null && section.ImagePaths.Count > 0)
            {
                if (!frame.IsUsed)
                {
                    LevelImage image = section.ImagePaths[0].LoadedImage;
                    frame.SetLevelImage(image);
                    section.ImagePaths.RemoveAt(0);
                }
            }
            else
            {
                frame.gameObject.SetActive(false);
            }
        }

        // Place the podium images and text
        foreach (Placeholder podiumPlaceholder in this.DisplayPodiums)
        {
            if (podiumPlaceholder.PartType == Placeholder.RoomPartType.DisplayPodium && section.PodiumImages.Count > 0)
            {
                DisplayPodium podium = podiumPlaceholder.GetInstance<DisplayPodium>();
                LevelImage image = section.PodiumImages[0].LoadedImage;
                podium.SetImage(image);
                section.PodiumImages.RemoveAt(0);
            }
            else if (podiumPlaceholder.PartType == Placeholder.RoomPartType.TextPodium && section.LocationText.Count > 0)
            {
                DisplayPodium podium = podiumPlaceholder.GetInstance<DisplayPodium>();
                podium.SetText(section.LocationText[0], section.Title);
                section.LocationText.RemoveAt(0);
            }
            else
            {
                podiumPlaceholder.gameObject.SetActive(false);
            }
        }

        // Table of Contents podium
        if (this.TOCPodium != null)
        {
            if (section.TableOfContents != null)
            {
                TableOfContentsPodium podium = this.TOCPodium.GetInstance<TableOfContentsPodium>();
                podium.SetTableOfContents(section.TableOfContents);
                section.TableOfContents = null;
            }
            else
            {
                this.TOCPodium.gameObject.SetActive(false);
            }
        }

        // Set the text
        foreach (Placeholder bookPlaceholder in this.Reading)
        {
            if (section.LocationText != null && section.LocationText.Count > 0)
            {
                ReadingContent book = bookPlaceholder.GetInstance<ReadingContent>();
                book.AddText(section.LocationText[0]);
                section.LocationText.RemoveAt(0);
            }
            else
            {
                bookPlaceholder.gameObject.SetActive(false);
            }
        }

        var exits = section.LinkedLocationData.Where(a => a.Type == LinkedLocationDataType.DoorLink).ToList();
        foreach (var door in this.Exits)
        {
            if (exits.Count > 0)
            {
                // Assign the first linked location data to the exit
                LinkedLocationData linkedData = exits[0];
                door.SetLocationData(linkedData);
                exits.RemoveAt(0);
            }
            else
            {
                door.gameObject.SetActive(false);
            }
        }

        // Populate subexhibits recursively
        if (SubExhibits.Length < data.SubExhibitData.Count)
        {
            Debug.LogWarning($"Exhibit {this.name} has fewer subexhibits ({SubExhibits.Length}) than required by the data ({data.SubExhibitData.Count}). Some data will not match a subexhibit.");
        }

        for (int i = 0; i < SubExhibits.Length; ++i)
        {
            if (data.SubExhibitData.Count <= i || data.SubExhibitData[i] == null)
            {
                SubExhibits[i].ClearAssignment();
                SubExhibits[i].gameObject.SetActive(false);
                continue;
            }
            SubExhibits[i].PopulateExhibit(data.SubExhibitData[i]);
        }
    }

    // Handles list items in section.Lists according to ListItemMode
    private void HandleListItems()
    {
        if (section == null || section.Lists == null || section.Lists.Count == 0)
        {
            return;
        }

        switch (ListItemMode)
        {
            case ExhibitListItemMode.Skip:
                Debug.LogWarning($"Exhibit {this.name}: List items present in section '{section.Title}' but ExhibitListItemMode is Skip. Lists will be ignored.");
                break;
            case ExhibitListItemMode.CombineWithParagraphs:
                // Each list item becomes a paragraph (LocationTextData)
                foreach (var list in section.Lists)
                {
                    if (list?.Items != null)
                        section.LocationText.AddRange(list.Items);
                }
                section.Lists.Clear();
                break;
            case ExhibitListItemMode.CombinedIntoOneParagraph:
                // All list items are merged into a single paragraph
                var combined = new System.Text.StringBuilder();
                foreach (var list in section.Lists)
                {
                    if (list?.Items != null)
                    {
                        foreach (var item in list.Items)
                        {
                            if (!string.IsNullOrWhiteSpace(item.Text))
                            {
                                if (combined.Length > 0)
                                {
                                    combined.Append("\n");
                                }
                                combined.Append("• ").Append(item.Text);
                            }
                        }
                    }
                }
                if (combined.Length > 0)
                {
                    section.LocationText.Add(new LocationTextData(combined.ToString()));
                }

                section.Lists.Clear();
                break;
        }
    }


    /// <summary>
    /// Rates how well this Exhibit matches the given SectionData, including subexhibits. Does not mutate the Exhibit.
    /// </summary>
    public override RatingResult RateSectionMatch(SectionData section)
    {
        if (section == null)
        {
            return RatingResult.NoMatch;
        }

        float score = 5f;

        // SectionType match is required
        if (!CanHandleSection(section))
        {
            return RatingResult.NoMatch;
        }

        // Title/AreaTitleSign
        bool hasTitle = !string.IsNullOrEmpty(section.Title);
        if (hasTitle)
        {
            score += this.SupportsTitle() ? 1f : -1f;
        }

        // Reading placeholders vs LocationText
        int textCount = section.LocationText?.Count ?? 0;
        int readingCount = this.GetReadingCount();
        score += ScoreCountMatch(readingCount, textCount, 1f);

        int imagePodiumCount = this.DisplayPodiums.Count(a => a.CanHandlePodiumImage);
        score += ScoreCountMatch(this.Paintings.Length, section.ImagePaths.Count, 2f);
        score += ScoreCountMatch(imagePodiumCount, section.PodiumImages.Count, 2f);
        score += ScoreCountMatch(this.Exits.Count, section.Exits.Count, 2f);

        // Subsections and subexhibits
        score += ScoreSubsections(section);

        return new RatingResult(score, true);
    }

    private bool SupportsTitle()
    {
        return this.AreaTitleSign != null ||
            this.DisplayPodiums.Any(p => p.PartType == Placeholder.RoomPartType.TextPodium);
    }

    private int GetReadingCount()
    {
        return this.Reading.Length + this.DisplayPodiums.Count(a => a.CanHandleText);
    }

    private float ScoreSubsections(SectionData section)
    {
        var score = 0.0f;
        if (section.Subsections.Count > 0)
        {
            return score;
        }

        if (SubExhibits.Length == 0)
        {
            // dock points for missing subsections
            score -= section.Subsections.Count;
            return score;
        }
        float subScore = 0f;
        int matched = 0;
        foreach (var subSection in section.Subsections)
        {
            float best = 0f;
            foreach (var sub in SubExhibits)
            {
                var result = sub.RateSectionMatch(subSection);
                if (!result.IsValid)
                {
                    // skip invalid results
                    continue;
                }

                if (result.Score > best)
                {
                    best = result.Score;
                }
            }
            subScore += best;
            if (best > 0)
            {
                matched++;
            }
        }

        score += subScore;
        if (matched == section.Subsections.Count)
        {
            score += 1f;
        }
       
        return score;
    }

    /// <summary>
    /// Helper to score how well two counts match. Prefers exact match, then too many, then too few. The more difference, the worse the score.
    /// </summary>
    private float ScoreCountMatch(int exhibitCount, int requiredCount, float weight)
    {
        if (requiredCount == 0)
        {
            return 0f;
        }

        if (requiredCount > 0 && exhibitCount == 0)
        {
            // If required count is > 0 but we have none, this is a bad match
            return -weight;
        }

        int diff = Math.Abs(exhibitCount - requiredCount);
        if (diff == 0)
        {
            return weight; // perfect match
        }
        else if (diff > 0)
        {
            // Too many: small penalty per extra
            return weight - (0.25f * diff * weight);
        }
        else
        {
            // Too few: larger penalty per missing
            return weight - (0.5f * diff * weight);
        }
    }
}
