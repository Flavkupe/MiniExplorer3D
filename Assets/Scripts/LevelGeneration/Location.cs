

using System;

[Serializable]
public abstract class Location 
{
	public string Path;
	public string Name;
    public string Anchor { get; set; }

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

    public virtual bool IsRandomLocation => false;

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


[Serializable]
public class RandomLocation : MainLocation
{
    public RandomLocation() : base(string.Empty, "Random") { }

    public override bool IsBackLocation { get { return false; } }

    public override bool IsRandomLocation => true;

    public override bool NeedsInitialization => true;

    public override string LocationKey => "Random";

    public override Location Clone(bool deepClone = false)
    {
        return new RandomLocation{};
    }
}
