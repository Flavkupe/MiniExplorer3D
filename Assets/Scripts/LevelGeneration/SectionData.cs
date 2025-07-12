using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

[Flags]
public enum SectionType
{
    None = 0,

    /// <summary>
    /// Main room, usually content under the main h1 header.
    /// </summary>
    Main = 1 << 0,

    /// <summary>
    /// A full exhibit that is not the main room;
    /// Usually inside an h2 header.
    /// </summary>
    Standard = 1 << 1,
    Table = 1 << 2,
    Infobox = 1 << 3,
    Gallery = 1 << 4,
    Other = 1 << 5,
    
    /// <summary>
    /// Section meant for a sub-exhibit
    /// </summary>
    Subsection = 1 << 6,

    /// <summary>
    /// See also sections of articles, and similar
    /// </summary>
    SeeAlso = 1 << 7,

    /// <summary>
    /// Exhibit for segments like references or external links, or similar
    /// </summary>
    References = 1 << 8,
}

public class SectionData
{
    public string Title { get; set; }
    public string Anchor { get; set; }
    public SectionType SectionType { get; set; }
    public List<SectionData> Subsections { get; private set; } = new List<SectionData>();
    public List<LocationTextData> LocationText { get; private set; } = new List<LocationTextData>();
    public List<ImagePathData> ImagePaths { get; private set; } = new List<ImagePathData>();
    public List<LinkedLocationData> LinkedLocationData { get; private set; } = new List<LinkedLocationData>();
    public TableOfContents TableOfContents { get; set; }
    public string RawData { get; set; }
    public List<ListItemsData> Lists { get; set; } = new List<ListItemsData>();

    public ReadOnlyCollection<LinkedLocationData> Exits
    {
        get
        {
            return new ReadOnlyCollection<LinkedLocationData>(LinkedLocationData.Where(a => a.Type == LinkedLocationDataType.DoorLink).ToList());
        }
    }

    public bool IsEmpty()
    {
        return this.Subsections.Count == 0 &&
            this.LocationText.Count == 0 &&
            this.ImagePaths.Count == 0 &&
            this.LinkedLocationData.Count == 0 &&
            this.TableOfContents == null &&
            this.Lists.Count == 0;
    }

    public SectionData Clone()
    {
        return new SectionData
        {
            Title = Title,
            Anchor = Anchor,
            SectionType = SectionType,
            RawData = RawData,
            TableOfContents = TableOfContents,
            LocationText = LocationText.Select(a => a.Clone()).ToList(),
            ImagePaths = ImagePaths.Select(a => a.Clone()).ToList(),
            LinkedLocationData = LinkedLocationData.Select(a => a.Clone()).ToList(),
            Subsections = Subsections.Select(a => a.Clone()).ToList(),
            Lists = Lists.Select(a => a.Clone()).ToList(),
        };
    }
}
