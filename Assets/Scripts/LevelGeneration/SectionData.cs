using System;
using System.Collections.Generic;

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
    // Add more as needed
}

public class SectionData
{
    public string Title { get; set; }
    public string Anchor { get; set; }
    public SectionType SectionType { get; set; }
    public List<SectionData> Subsections { get; set; } = new List<SectionData>();
    public List<LocationTextData> LocationText { get; set; } = new List<LocationTextData>();
    public List<ImagePathData> ImagePaths { get; set; } = new List<ImagePathData>();
    public List<ImagePathData> PodiumImages { get; set; } = new List<ImagePathData>();
    public List<LinkedLocationData> LinkedLocationData { get; set; } = new List<LinkedLocationData>();
    public List<InfoBoxData> InfoBoxData { get; set; } = new List<InfoBoxData>();
    public TableOfContents TableOfContents { get; set; }
    public string RawData { get; set; }
}
