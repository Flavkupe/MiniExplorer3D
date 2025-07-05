using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Exhibit : MonoBehaviour, IMatchesPrefab
{
    // Arbitrary limit for reading content
    const int SkipContentLimit = 8;

    public SectionType SectionTypes;

    private SectionData assignedSection;
    public bool IsAssigned { get; private set; }

    public string PrefabID => this.name;

    private RoomImageFrame[] Paintings = new RoomImageFrame[] { };
    private Placeholder[] Reading = new Placeholder[] { };
    private Placeholder[] DisplayPodiums = new Placeholder[] { };
    private Placeholder TOCPodium = null;
    private AreaTitle AreaTitleSign = null;
    public Exhibit[] SubExhibits { get; private set; } = new Exhibit[] { };

    public void PopulateParts()
    {
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
        this.SubExhibits = children.SelectMany(go => go.GetComponents<Exhibit>()).Where(e => e != this).ToArray();
    }

    public bool CanHandleSection(SectionData section)
    {
        if (section == null)
            return false;
        // SectionType flag check
        if ((SectionTypes & section.SectionType) == 0)
            return false;
        // Rule 1: AreaTitleSign iff Title is non-empty
        bool hasTitle = !string.IsNullOrEmpty(section.Title);
        if (hasTitle != (this.AreaTitleSign != null))
            return false;
        // Rule 2: Enough Reading items for LocationText
        int textCount = section.LocationText != null ? section.LocationText.Count : 0;
        int readingCount = this.Reading != null ? this.Reading.Length : 0;
        if (readingCount < textCount)
        {
            if (textCount > SkipContentLimit)
            {
                Debug.LogWarning($"Exhibit {this.name} has too many reading items ({textCount}) for the available placeholders ({readingCount}). Skipping some content.");
            }
            else
            {
                return false;
            }
        }
        // Rule 3: Subsections must be handled by subexhibits
        if (section.Subsections != null && section.Subsections.Count > 0)
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

    public void ClearAssignment()
    {
        assignedSection = null;
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
    public virtual void PopulateExhibit(ExhibitData data)
    {
        PopulateParts();
        this.assignedSection = data?.SectionData;
        if (this.assignedSection == null)
        {
            this.gameObject.SetActive(false);
            return;
        }
        IsAssigned = true;

        // Example: Set AreaTitle if present
        if (this.AreaTitleSign != null && !string.IsNullOrEmpty(assignedSection.Title))
        {
            this.AreaTitleSign.SetTitle(assignedSection.Title);
        }

        // Place the images
        foreach (RoomImageFrame frame in this.Paintings)
        {
            if (assignedSection.ImagePaths != null && assignedSection.ImagePaths.Count > 0)
            {
                if (!frame.IsUsed)
                {
                    LevelImage image = assignedSection.ImagePaths[0].LoadedImage;
                    frame.SetLevelImage(image);
                    assignedSection.ImagePaths.RemoveAt(0);
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
            if (podiumPlaceholder.PartType == Placeholder.RoomPartType.DisplayPodium && assignedSection.PodiumImages != null && assignedSection.PodiumImages.Count > 0)
            {
                DisplayPodium podium = podiumPlaceholder.GetInstance<DisplayPodium>();
                LevelImage image = assignedSection.PodiumImages[0].LoadedImage;
                podium.SetImage(image);
                assignedSection.PodiumImages.RemoveAt(0);
            }
            else if (podiumPlaceholder.PartType == Placeholder.RoomPartType.TextPodium && assignedSection.InfoBoxData != null && assignedSection.InfoBoxData.Count > 0)
            {
                DisplayPodium podium = podiumPlaceholder.GetInstance<DisplayPodium>();
                podium.SetText(assignedSection.InfoBoxData[0]);
                assignedSection.InfoBoxData.RemoveAt(0);
            }
            else
            {
                podiumPlaceholder.gameObject.SetActive(false);
            }
        }

        // Table of Contents podium
        if (this.TOCPodium != null)
        {
            if (assignedSection.TableOfContents != null)
            {
                TableOfContentsPodium podium = this.TOCPodium.GetInstance<TableOfContentsPodium>();
                podium.SetTableOfContents(assignedSection.TableOfContents);
                assignedSection.TableOfContents = null;
            }
            else
            {
                this.TOCPodium.gameObject.SetActive(false);
            }
        }

        // Set the text
        foreach (Placeholder bookPlaceholder in this.Reading)
        {
            if (assignedSection.LocationText != null && assignedSection.LocationText.Count > 0)
            {
                ReadingContent book = bookPlaceholder.GetInstance<ReadingContent>();
                book.AddText(assignedSection.LocationText[0]);
                assignedSection.LocationText.RemoveAt(0);
            }
            else
            {
                bookPlaceholder.gameObject.SetActive(false);
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
}
