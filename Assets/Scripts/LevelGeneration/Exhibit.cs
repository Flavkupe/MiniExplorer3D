using System;
using UnityEngine;
using System.Linq;

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

    public void PopulateParts()
    {
        var allPlaceholders = this.GetComponentsInChildren<Placeholder>(true).ToList();
        this.Paintings = this.GetComponentsInChildren<RoomImageFrame>(true).Where(a => a.FrameType == RoomImageFrame.ImageFrameType.Painting).ToArray();
        this.Reading = allPlaceholders.Where(a => a.PartType == Placeholder.RoomPartType.Reading).ToArray();
        this.DisplayPodiums = allPlaceholders.Where(a => a.PartType == Placeholder.RoomPartType.DisplayPodium ||
                                                         a.PartType == Placeholder.RoomPartType.TextPodium).ToArray();
        this.TOCPodium = allPlaceholders.FirstOrDefault(a => a.PartType == Placeholder.RoomPartType.TableOfContentsPodium);
        this.AreaTitleSign = this.GetComponentInChildren<AreaTitle>();
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
                // Arbitrary limit for reading content
                Debug.LogWarning($"Exhibit {this.name} has too many reading items ({textCount}) for the available placeholders ({readingCount}). Skipping some content.");
            }
            else
            {
                return false;
            }
        }
            
        return true;
    }

    public void ClearAssignment()
    {
        assignedSection = null;
        IsAssigned = false;
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
    }
}
