using Assets.Scripts.LevelGeneration;
using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class DebugLevelGenerator : WebLevelGenerator
{    
    public DebugLevelGenerator()
    {
    }

    protected override void ProcessLocation(Location parentLocation)
    {               
        if (!(parentLocation is MainLocation))
        {            
            return;
        }

        MainLocation location = parentLocation as MainLocation;

        LocationTextData textData = new LocationTextData("aaa");
        textData.LinkedLocationData.Add(new LinkedLocationData("DataName1", "DataPath1"));
        textData.LinkedLocationData.Add(new LinkedLocationData("DataName2", "DataPath2"));
        textData.LinkedLocationData.Add(new LinkedLocationData("DataName3", "DataPath3"));
        location.LocationData.LocationText.Add(textData);
        location.LocationData.LocationText.Add(new LocationTextData("bbb"));
        location.LocationData.ImagePaths.Add(new ImagePathData("MyImg", "Debug/DsnSo"));

        SubLocation loc1 = new SubLocation(parentLocation, "Loc1");        
        SubLocation loc2 = new SubLocation(parentLocation, "Loc2");
        SubLocation loc3 = new SubLocation(parentLocation, "Loc3");
        SubLocation subloc1 = new SubLocation(parentLocation, "SubLoc1");
        SubLocation subloc2 = new SubLocation(parentLocation, "SubLoc2");
        loc1.LocationData.SubLocations.Add(subloc1);
        loc1.LocationData.SubLocations.Add(subloc2);
        loc1.LocationData.LocationText.Add(new LocationTextData("123"));
        LocationTextData textData2 = new LocationTextData("456");
        textData2.LinkedLocationData.Add(new LinkedLocationData("DataName1", "DataPath1"));
        textData2.LinkedLocationData.Add(new LinkedLocationData("DataName2", "DataPath2"));
        textData2.LinkedLocationData.Add(new LinkedLocationData("DataName3", "DataPath3"));
        loc1.LocationData.LocationText.Add(textData2);

        location.LocationData.SubLocations.Add(loc1);
        location.LocationData.SubLocations.Add(loc2);
        location.LocationData.SubLocations.Add(loc3);
                       
        return;
    }

    protected override IEnumerator ProcessImages(Location location)
    {               
        foreach (ImagePathData imageData in location.LocationData.ImagePaths)
        {
            LevelImage levelImage = new LevelImage() { Name = imageData.DisplayName };
            Texture2D tex = Resources.Load<Texture2D>(imageData.Path);
            levelImage.Texture2D = tex;
            imageData.LoadedImage = levelImage;        
        }

        yield return null;
    }

    public override IEnumerator PrepareAreaGeneration(Location location, MonoBehaviour caller)
    {
        this.CallOnAreaGenReady(new AreaGenerationReadyEventArgs() { AreaLocation = StageManager.CurrentLocation });
        yield return null;
    }

    public override bool NeedsAreaGenPreparation { get { return true; } }
}