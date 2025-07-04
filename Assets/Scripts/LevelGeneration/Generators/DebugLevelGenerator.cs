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

    protected override void ProcessHtmlDocument(MainLocation location, Uri currentUri)
    {
        // No-op for debug generator
    }

    protected override void ProcessLocation(Location parentLocation)
    {               
        if (!(parentLocation is MainLocation))
        {            
            return;
        }

        MainLocation location = parentLocation as MainLocation;

        // Create a single debug section
        SectionData debugSection = new SectionData
        {
            Title = "Debug Section",
            SectionType = SectionType.Standard
        };

        LocationTextData textData = new LocationTextData("aaa");
        textData.LinkedLocationData.Add(new LinkedLocationData("DataName1", "DataPath1"));
        textData.LinkedLocationData.Add(new LinkedLocationData("DataName2", "DataPath2"));
        textData.LinkedLocationData.Add(new LinkedLocationData("DataName3", "DataPath3"));
        debugSection.LocationText.Add(textData);
        debugSection.LocationText.Add(new LocationTextData("bbb"));
        debugSection.ImagePaths.Add(new ImagePathData("MyImg", "Debug/DsnSo"));

        // SubLocations
        SubLocation loc1 = new SubLocation(parentLocation, "Loc1");        
        SubLocation loc2 = new SubLocation(parentLocation, "Loc2");
        SubLocation loc3 = new SubLocation(parentLocation, "Loc3");
        SubLocation subloc1 = new SubLocation(parentLocation, "SubLoc1");
        SubLocation subloc2 = new SubLocation(parentLocation, "SubLoc2");
        loc1.LocationData.SubLocations.Add(subloc1);
        loc1.LocationData.SubLocations.Add(subloc2);
        SectionData loc1Section = new SectionData { Title = "Loc1 Section", SectionType = SectionType.Standard };
        loc1Section.LocationText.Add(new LocationTextData("123"));
        LocationTextData textData2 = new LocationTextData("456");
        textData2.LinkedLocationData.Add(new LinkedLocationData("DataName1", "DataPath1"));
        textData2.LinkedLocationData.Add(new LinkedLocationData("DataName2", "DataPath2"));
        textData2.LinkedLocationData.Add(new LinkedLocationData("DataName3", "DataPath3"));
        loc1Section.LocationText.Add(textData2);
        loc1.LocationData.Sections.Add(loc1Section);

        location.LocationData.SubLocations.Add(loc1);
        location.LocationData.SubLocations.Add(loc2);
        location.LocationData.SubLocations.Add(loc3);

        // Add the debug section to the main location
        location.LocationData.Sections.Add(debugSection);
        return;
    }

    protected override IEnumerator ProcessImages(Location location)
    {               
        // Traverse SectionData for all images
        List<ImagePathData> imagePaths = new List<ImagePathData>();
        if (location.LocationData?.Sections != null)
        {
            foreach (var section in location.LocationData.Sections)
            {
                CollectImagesFromSection(section, imagePaths);
            }
        }
        foreach (ImagePathData imageData in imagePaths)
        {
            LevelImage levelImage = new LevelImage() { Name = imageData.DisplayName };
            Texture2D tex = Resources.Load<Texture2D>(imageData.Path);
            levelImage.Texture2D = tex;
            imageData.LoadedImage = levelImage;        
        }

        yield return null;
    }

    private void CollectImagesFromSection(SectionData section, List<ImagePathData> imagePaths)
    {
        if (section == null) return;
        if (section.ImagePaths != null)
            imagePaths.AddRange(section.ImagePaths);
        if (section.PodiumImages != null)
            imagePaths.AddRange(section.PodiumImages);
        if (section.Subsections != null)
        {
            foreach (var sub in section.Subsections)
                CollectImagesFromSection(sub, imagePaths);
        }
    }

    public override IEnumerator PrepareAreaGeneration(Location location, MonoBehaviour caller)
    {
        this.CallOnAreaGenReady(new AreaGenerationReadyEventArgs() { AreaLocation = StageManager.CurrentLocation });
        yield return null;
    }

    public override bool NeedsAreaGenPreparation { get { return true; } }
}