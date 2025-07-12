using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

public class Exhibit : ExhibitBase, ICanSupportTitle
{
    private SectionData section;

    /// <summary>
    /// Clone of the original section, for debug purposes
    /// </summary>
    private SectionData originalSection;

    [Tooltip("How list items are displayed.")]
    public ExhibitListItemMode ListItemMode = ExhibitListItemMode.Skip;

    public List<RoomImageFrame> Paintings { get; private set; } = new();
    public List<Placeholder> PaintingPlaceholders { get; private set; } = new();

    public List<ReadingContent> Reading { get; private set; } = new();
    public List<Placeholder> ReadingPlaceholders { get; private set; } = new();

    public List<AreaTitle> AreaTitleSigns { get; private set; } = new();

    public Placeholder TOCPodium = null;
    public List<Door> Exits { get; private set; } = new();

    public List<ExhibitBase> SubExhibits { get; private set; } = new();

    public override void PopulateParts()
    {
        // Only get immediate children for Placeholders, RoomImageFrame, and SubExhibits
        var children = new List<GameObject>();
        for (int i = 0; i < transform.childCount; ++i)
        {
            children.Add(transform.GetChild(i).gameObject);
        }

        var allPlaceholders = children.SelectMany(go => go.GetComponents<Placeholder>()).ToList();
        this.Paintings = children.SelectMany(go => go.GetComponents<RoomImageFrame>()).ToList();
        this.Reading = children.SelectMany(go => go.GetComponents<ReadingContent>()).ToList();
        this.AreaTitleSigns = children.SelectMany(go => go.GetComponents<AreaTitle>()).ToList();
        this.Exits = children.SelectMany(go => go.GetComponents<Door>()).ToList();

        this.PaintingPlaceholders = allPlaceholders.Where(a => a.PartType == Placeholder.RoomPartType.ImageFrame).ToList();
        this.ReadingPlaceholders = allPlaceholders.Where(a => a.PartType == Placeholder.RoomPartType.Reading).ToList();

        this.TOCPodium = allPlaceholders.FirstOrDefault(a => a.PartType == Placeholder.RoomPartType.TableOfContentsPodium);
        this.SubExhibits = children.SelectMany(go => go.GetComponents<ExhibitBase>()).Where(e => e != this).ToList();

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

        // For text, must have at least one piece of text content
        if (section.LocationText.Count > 0 && this.GetReadingCount() == 0)
        {
            return false;
        }

        // Subsections must be handled by subexhibits
        if (section.Subsections.Count > 0)
        {
            if (SubExhibits == null || SubExhibits.Count == 0)
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

        this.originalSection = this.section.Clone();

        IsAssigned = true;

        // Handle list items according to the mode
        HandleListItems();

        // Example: Set AreaTitle if present
        PopulateAreaTitle();

        // Place the images
        PopulateImages();

        // Set the text
        PopulateText();

        // Set the exits
        PopulateExits();

        // Table of Contents podium
        if (this.TOCPodium != null)
        {
            if (section.TableOfContents != null)
            {
                TableOfContentsPodium podium = this.TOCPodium.ReplaceInstance<TableOfContentsPodium>();
                podium.SetTableOfContents(section.TableOfContents);
                section.TableOfContents = null;
            }
            else
            {
                this.TOCPodium.gameObject.SetActive(false);
            }
        }


        // Populate subexhibits recursively
        if (SubExhibits.Count < data.SubExhibitData.Count)
        {
            Debug.LogWarning($"Exhibit {this.PrefabID} has fewer subexhibits ({SubExhibits.Count}) than required by the data ({data.SubExhibitData.Count}). Some data will not match a subexhibit.");
        }

        for (int i = 0; i < SubExhibits.Count; ++i)
        {
            if (data.SubExhibitData.Count <= i || data.SubExhibitData[i] == null)
            {
                SubExhibits[i].ReplaceWithUnused();
                continue;
            }
            SubExhibits[i].PopulateExhibit(data.SubExhibitData[i]);
        }
    }

    private void PopulateExits()
    {
        var exits = section.LinkedLocationData.Where(a => a.Type == LinkedLocationDataType.DoorLink).ToList();
        foreach (var door in this.Exits)
        {
            if (exits.Count > 0)
            {
                // Assign the first linked location data to the exit
                LinkedLocationData linkedData = exits[0];
                door.SetLocationData(linkedData);
                door.gameObject.SetActive(true);
                exits.RemoveAt(0);
            }
            else
            {
                door.gameObject.SetActive(false);
            }
        }
    }

    private void PopulateAreaTitle()
    {
        foreach (var titleSign in this.AreaTitleSigns)
        {
            if (section.Title == null || section.Title.Length == 0)
            {
                titleSign.gameObject.SetActive(false);
                continue;
            }
            titleSign.SetTitle(section.Title ?? string.Empty);
        }
    }

    private void PopulateImages()
    {
        foreach (RoomImageFrame frame in this.Paintings)
        {
            if (section.ImagePaths != null && section.ImagePaths.Count > 0)
            {
                if (!frame.IsUsed)
                {
                    LevelImage image = section.ImagePaths[0].LoadedImage;
                    frame.SetLevelImage(image);
                    frame.gameObject.SetActive(true);
                    section.ImagePaths.RemoveAt(0);
                }
            }
            else
            {
                frame.gameObject.SetActive(false);
            }
        }

        foreach (Placeholder placeholder in this.PaintingPlaceholders)
        {
            if (section.ImagePaths.Count > 0)
            {
                var frame = placeholder.ReplaceInstance<RoomImageFrame>();
                if (frame == null)
                {
                    Debug.LogError($"Placeholder {placeholder.name} could not instantiate RoomImageFrame.");
                    continue;
                }
                LevelImage image = section.ImagePaths[0].LoadedImage;
                frame.SetLevelImage(image);
                frame.gameObject.SetActive(true);
                section.ImagePaths.RemoveAt(0);
            }
            else
            {
                placeholder.gameObject.SetActive(false);
            }
        }
    }

    private void PopulateText()
    {
        foreach (ReadingContent reading in this.Reading)
        {
            if (section.LocationText.Count > 0)
            {
                var locationText = section.LocationText[0];
                reading.AddText(locationText);
                reading.gameObject.SetActive(true);
                section.LocationText.RemoveAt(0);
            }
            else
            {
                reading.gameObject.SetActive(false);
            }
        }

        foreach (Placeholder placeholder in this.ReadingPlaceholders)
        {
            if (section.LocationText.Count > 0)
            {
                ReadingContent reading = placeholder.ReplaceInstance<ReadingContent>();
                if (reading == null)
                {
                    Debug.LogError($"Placeholder {placeholder.name} could not instantiate ReadingContent.");
                    continue;
                }

                reading.AddText(section.LocationText[0]);
                reading.gameObject.SetActive(true);
                section.LocationText.RemoveAt(0);
            }
            else
            {
                placeholder.gameObject.SetActive(false);
            }
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
                Debug.LogWarning($"Exhibit {this.PrefabID}: List items present in section '{section.Title}' but ExhibitListItemMode is Skip. Lists will be ignored.");
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
        return RatingProcessor.RateExhibitMatch(this, section);
    }

    public bool SupportsTitle => this.AreaTitleSigns.Count > 0 || this.Reading.Any(a => a.SupportsTitle) || this.ReadingPlaceholders.Any(a => a.SupportsTitle);

    public int GetReadingCount()
    {
        return this.Reading.Count + this.ReadingPlaceholders.Count;
    }

    public int GetPaintingCount()
    {
        return this.Paintings.Count + this.PaintingPlaceholders.Count;
    }

    [ContextMenu("Print Section to console")]
    public void PrintSectionToConsole()
    {
        if (this.originalSection == null)
        {
            Debug.LogWarning("Section is null.");
            return;
        }
        try
        {
            string json = JsonConvert.SerializeObject(this.originalSection, Formatting.Indented);
            Debug.Log(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to serialize section: {ex.Message}");
        }
    }

}
