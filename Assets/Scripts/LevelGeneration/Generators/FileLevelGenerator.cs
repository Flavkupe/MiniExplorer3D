using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.LevelGeneration;

public class FileLevelGenerator : BaseLevelGenerator 
{
    protected override List<Location> GetBranchLocations(Location location) 
	{
		string[] directories = Directory.GetDirectories(location.Path);
		List<Location> locations = new List<Location>();
		foreach (string directory in directories) 
		{
            locations.Add(new MainLocation(directory, Path.GetFileName(directory)));
		}

		return locations;
	}

    public override bool CanLoadLocation(Location location)
    {
        try
        {
            Directory.GetDirectories(location.Path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override List<string> GetLevelEntities(Location location)
    {
        return Directory.GetFiles(location.Path).Select(a => Path.GetFileName(a)).ToList();
    }

    protected override Location GetBackLocation(Location currentLocation) 
	{
		DirectoryInfo parent = Directory.GetParent(currentLocation.Path);
		if (parent == null) 
		{
			return null;
		}

        return new BackLocation(parent.FullName, "..");	 
	}


    public override List<LevelImage> GetLevelImages(Location location)
    {
        return new List<LevelImage>();
    }

    protected override AreaTheme GetAreaTheme(Location location)
    {
        // TODO
        return AreaTheme.Circuit;
    }
}
