using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static System.Collections.Specialized.BitVector32;

[Flags]
public enum SectionType
{
    None = 0,
    Main = 1 << 0,
    Standard = 1 << 1,
    Table = 1 << 2,
    Infobox = 1 << 3,
    Gallery = 1 << 4,
    Other = 1 << 5,
    Subsection = 1 << 6,
}

public class SectionData
{
    public string Title { get; set; }
    public string Anchor { get; set; }
    public SectionType SectionType { get; set; }
    public List<SectionData> Subsections { get; private set; } = new List<SectionData>();
    public List<LocationTextData> LocationText { get; private set; } = new List<LocationTextData>();
    public List<ImagePathData> ImagePaths { get; private set; } = new List<ImagePathData>();
    public List<ImagePathData> PodiumImages { get; private set; } = new List<ImagePathData>();
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
            PodiumImages = PodiumImages.Select(a => a.Clone()).ToList(),
            LinkedLocationData = LinkedLocationData.Select(a => a.Clone()).ToList(),
            Subsections = Subsections.Select(a => a.Clone()).ToList(),
        };
    }
}
