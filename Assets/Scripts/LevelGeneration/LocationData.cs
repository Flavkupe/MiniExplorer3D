using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LocationData
{
    public void Clear()
    {
        this.locationText.Clear();
        this.imagePaths.Clear();
        this.subLocations.Clear();
        this.linkedLocationData.Clear();
    }

    private List<string> locationText = new List<string>();
    public List<string> LocationText
    {
        get { return locationText; }
    }

    private List<ImagePathData> imagePaths = new List<ImagePathData>();
    public List<ImagePathData> ImagePaths
    {
        get { return imagePaths; }
    }

    private List<LinkedLocationData> linkedLocationData = new List<LinkedLocationData>();
    public List<LinkedLocationData> LinkedLocationData
    {
        get { return linkedLocationData; }
    }

    public List<Location> subLocations = new List<Location>();
    public List<Location> SubLocations { get { return this.subLocations; } }

    // For stuff like html markup
    public string RawData { get; set; }


    public LocationData Clone()
    {
        LocationData clone = new LocationData();
        clone.subLocations = this.subLocations.Select(a => a.Clone()).ToList();
        clone.imagePaths = this.imagePaths.ToList();
        clone.locationText = this.locationText.ToList();
        clone.linkedLocationData = this.linkedLocationData.ToList();
        return clone;
    }
}

public class ImagePathData
{
    public string DisplayName { get; set; }
    public string Path { get; set; }
    public ImagePathData(string display, string path)
    {
        this.DisplayName = display;
        this.Path = path;
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
}


