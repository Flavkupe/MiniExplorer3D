using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LocationData
{
    public void Clear()
    {
        this.Sections.Clear();
        this.subLocations.Clear();
    }

    public List<Location> subLocations = new List<Location>();
    public List<Location> SubLocations { get { return this.subLocations; } }

    public TableOfContents TableOfContents { get; set; }

    // For stuff like html markup
    public string RawData { get; set; }

    public List<SectionData> Sections { get; set; } = new List<SectionData>();

    public LocationData Clone()
    {
        LocationData clone = new LocationData();
        clone.subLocations = this.subLocations.Select(a => a.Clone()).ToList();
        clone.TableOfContents = this.TableOfContents;
        // Deep copy of Sections
        clone.Sections = this.Sections.Select(s => CloneSection(s)).ToList();
        clone.RawData = this.RawData;
        return clone;
    }

    private SectionData CloneSection(SectionData section)
    {
        var newSection = new SectionData
        {
            Title = section.Title,
            Anchor = section.Anchor,
            SectionType = section.SectionType,
            RawData = section.RawData,
            TableOfContents = section.TableOfContents,
            LocationText = section.LocationText != null ? new List<LocationTextData>(section.LocationText) : null,
            ImagePaths = section.ImagePaths != null ? new List<ImagePathData>(section.ImagePaths) : null,
            PodiumImages = section.PodiumImages != null ? new List<ImagePathData>(section.PodiumImages) : null,
            LinkedLocationData = section.LinkedLocationData != null ? new List<LinkedLocationData>(section.LinkedLocationData) : null,
            InfoBoxData = section.InfoBoxData != null ? new List<InfoBoxData>(section.InfoBoxData) : null,
            Subsections = section.Subsections != null ? section.Subsections.Select(s => CloneSection(s)).ToList() : null
        };
        return newSection;
    }
}

public class ImagePathData
{
    public string DisplayName { get; set; }
    public string Path { get; set; }

    public LevelImage LoadedImage { get; set; }

    public ImagePathData(string display, string path)
    {
        this.DisplayName = display;
        this.Path = path;
    }

    public ImagePathData Clone()
    {
        ImagePathData clone = new ImagePathData(this.DisplayName, this.Path);
        clone.LoadedImage = this.LoadedImage; // Copy the reference
        return clone;
    }
}

public class LocationTextData
{
    public string Text { get; set; }
    private List<LinkedLocationData> linkedLocationData = new List<LinkedLocationData>();
    public List<LinkedLocationData> LinkedLocationData { get { return this.linkedLocationData; } }
    public LocationTextData(string text)
    {
        this.Text = text;
    }

    public LocationTextData Clone()
    {
        LocationTextData clone = new LocationTextData(this.Text);
        this.linkedLocationData.ForEach(a => clone.linkedLocationData.Add(a.Clone()));
        return clone;
    }
}

public class LinkedLocationData
{
    public string DisplayName { get; set; }
    public string Path { get; set; }
    public LinkedLocationData(string display, string path)
    {
        this.DisplayName = display;
        this.Path = path;
    }

    public LinkedLocationData Clone()
    {
        return new LinkedLocationData(this.DisplayName, this.Path);
    }
}

public class TableOfContents
{
    public class TOCItem 
    {
        public string Rank { get; set; }
        public string Name { get; set; }
        public string Anchor { get; set; }
        public int Indentation { get; set; }
        public TOCItem(string name, string rank, string anchor, int indentation = 0)
        {            
            this.Name = name;
            this.Rank = rank;
            this.Anchor = anchor;
            this.Indentation = indentation;
        }
    }

    private List<TOCItem> tocItems = new List<TOCItem>();
    public List<TOCItem> TocItems { get { return this.tocItems; } }
}

public class InfoBoxData
{
    public string SectionTitle { get; set; }
    private List<string[]> rows = new List<string[]>();
    public List<string[]> Rows
    {
        get { return rows; }
    }

    public int Columns { get; set; }
    public InfoBoxData(int columns) 
    {
        this.Columns = columns;        
    }
}


