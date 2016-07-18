

using System;
using System.Collections.Generic;


[Serializable]
public abstract class Location 
{
	public string Path;
	public string Name;

    private LocationData locationData = new LocationData();
    public LocationData LocationData 
    { 
        get { return locationData; } 
        set { this.locationData = value; } 
    }

    public Location()
    {
    }

    public Location(string path, string name)
    {
        this.Path = path;
        this.Name = name;
    }

    public abstract bool NeedsInitialization { get; }
    public abstract bool IsBackLocation { get; }
    public abstract string LocationKey { get; }
    public abstract Location Clone(bool deepClone = false);

    public virtual Location GetParentLocation()
    {
        return null;
    }
}

[Serializable]
public class MainLocation : Location
{
    public override bool IsBackLocation { get { return false; } }

    public override Location Clone(bool deepClone = false) 
    {
        MainLocation loc = new MainLocation(this.Path, this.Name);        
        return loc;
    }

    public MainLocation()
        : base()
    {
    }

    public MainLocation(string path)
        : base(path, path)
    {
    }

    public MainLocation(string path, string name)
        : base(path, name)
    {
    }

    public override bool NeedsInitialization { get { return this.LocationData.RawData == null; } }
    public override string LocationKey { get { return this.Path; } }
}

[Serializable]
public class BackLocation : Location
{
    public override bool IsBackLocation { get { return true; } }

    public override Location Clone(bool deepClone = false)
    {
        return new BackLocation(this.Path, this.Name);
    }

    public BackLocation()
        : base()
    {
    }

    public BackLocation(string path, string name)
        : base(path, name)
    {
    }

    public override bool NeedsInitialization { get { return false; } }
    public override string LocationKey { get { return this.Path; } }
}

/// <summary>
/// A location that has a parent Location. The content
/// of the locations (text, images, etc) should be in here.
/// </summary>
[Serializable]
public class SubLocation : Location
{
    public override bool IsBackLocation { get { return false; } }

    public Location ParentLocation { get; set; }

    public override Location Clone(bool deepClone = false)
    {
        SubLocation loc = new SubLocation();
        loc.Name = this.Name;
        loc.Path = this.Path;
        if (deepClone)
        {
            loc.LocationData = this.LocationData.Clone();
            loc.ParentLocation = this.ParentLocation == null ? null : this.ParentLocation.Clone();
        }
        else
        {
            loc.LocationData = this.LocationData;
            loc.ParentLocation = this.ParentLocation;
        }

        return loc;
    }

    public SubLocation()
        : base()
    {
    }

    public SubLocation(Location parent, string name)
        : base(parent.Path, name)
    {
        this.ParentLocation = parent.Clone();
    }

    public override Location GetParentLocation()
    {
        return this.ParentLocation;
    }

    public override bool NeedsInitialization { get { return false; } }
    public override string LocationKey { get { return this.Path + "###" + this.Name; } }
}

