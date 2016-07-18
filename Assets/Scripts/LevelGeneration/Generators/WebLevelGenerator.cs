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

public class WebLevelGenerator : BaseLevelGenerator
{    
    private List<LevelImage> images = new List<LevelImage>();

    public WebLevelGenerator()
    {
    }

    protected override List<Location> GetBranchLocations(Location parentLocation)
    {               
        if (!(parentLocation is MainLocation))
        {
            // If this is not a MainLocation, the branches are just the precomputed sublinks and sublocations
            List<Location> linksAndSublocations = new List<Location>();
            linksAndSublocations.AddRange(parentLocation.LocationData.SubLocations);
            foreach (LinkedLocationData item in parentLocation.LocationData.LinkedLocationData)
            {
                linksAndSublocations.Add(new MainLocation(item.Path, item.DisplayName));
            }

            return linksAndSublocations;
        }

        MainLocation location = parentLocation as MainLocation;
        Uri currentUri = new Uri(location.Path);               

        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(location.LocationData.RawData);

        HtmlNode contentNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='mw-content-text']");
        HtmlNodeCollection subCategories = contentNode.SelectNodes("h2 | p | div/div/a/img | p/a");

        List<Location> sublocations = new List<Location>();
        SubLocation subLocation = null;

        location.LocationData.Clear();

        foreach (HtmlNode node in subCategories)
        {
            if (node.Name == "h2")
            {
                // Create sublocation for h2 header
                if (subLocation != null)
                {
                    sublocations.Add(subLocation);
                }
                
                string title = node.SelectSingleNode("span[@class='mw-headline']").InnerText;
                subLocation = new SubLocation(location, title);
                subLocation.LocationData.RawData = location.LocationData.RawData; // TEMP
            }
            else if (node.Name == "p")
            {
                string text = node.InnerText;
                // Store data from sublocation
                if (subLocation == null)
                {
                    // If no header seen yet, it's for the main article
                    location.LocationData.LocationText.Add(text);
                    continue;
                }
                else
                {                    
                    subLocation.LocationData.LocationText.Add(text);
                }
            }
            else if (node.Name == "img")
            {
                if (subLocation != null)
                {
                    string imageSrc = node.GetAttributeValue("src", "");
                    string imageCaption = node.GetAttributeValue("alt", "");
                    string imageUrl = "http://" + currentUri.Host + "/" + imageSrc.TrimStart('/');
                    subLocation.LocationData.ImagePaths.Add(new ImagePathData(imageCaption, imageUrl));
                }
            }
            else if (node.Name == "a")
            {
                if (subLocation != null)
                {
                    string href = node.GetAttributeValue("href", "");
                    string title = node.GetAttributeValue("title", "");
                    string url = "http://" + currentUri.Host + "/" + href.TrimStart('/');
                    subLocation.LocationData.LinkedLocationData.Add(new LinkedLocationData(title, url));
                }
            }
        }

        List<Location> allLocations = new List<Location>();
                
        // TODO: add links from opening paragraph

        allLocations.AddRange(sublocations);
        return allLocations;
    }

    private IEnumerator ProcessImages(Location location)
    {       
        if (location.LocationData.RawData == null)
        {
            yield return null;
        }

        Uri currentUri = new Uri(location.Path);
        images.Clear();
        int count = 0;
        MatchCollection imageMatches = Regex.Matches(location.LocationData.RawData, @"<img[^>]*src=\""([^\\""]+)\""[^>]*>");
        foreach (Match match in imageMatches)
        {
            if (count == 1) { continue; }
            if (match.Groups.Count < 2) { continue; }

            string cleanString = match.Groups[1].Value;

            LevelImage imageData = new LevelImage() { Name = cleanString };
            
            string url;
            if (cleanString.StartsWith("//")) 
            {
                url = "http://" + cleanString.TrimStart('/');
            }
            else
            {
                url = "http://" + currentUri.Host + "/" + cleanString.TrimStart('/');
            }
            

            WWW www = new WWW(url);
            yield return www;

            if (www.error != null)
            {
                continue;
            }

            if (www.texture != null)
            {
                imageData.Texture2D = new Texture2D(www.texture.width, www.texture.height);

                www.LoadImageIntoTexture(imageData.Texture2D);

                images.Add(imageData);
            }
        }

        this.CallOnAreaGenReady(new AreaGenerationReadyEventArgs() { AreaLocation = StageManager.CurrentLocation });
    }

    protected override Location GetBackLocation(Location currentLocation)
    {
        return currentLocation.GetParentLocation();
    }

    public override List<string> GetLevelEntities(Location location)
    {
        return new List<string>();
    }

    public override bool CanLoadLocation(Location location)
    {
        return true;
    }

    public override IEnumerator PrepareAreaGeneration(Location location, MonoBehaviour caller)
    {
        if (location.NeedsInitialization)
        {
            WWW www = new WWW(location.Path);
            yield return www;
            location.LocationData.RawData = www.text;
        }

        yield return caller.StartCoroutine(this.ProcessImages(location));        
    }

    public override bool NeedsAreaGenPreparation { get { return true; } }

    public override List<LevelImage> GetLevelImages(Location location)
    {
        return this.images;
    }

    protected override AreaTheme GetAreaTheme(Location location)
    {
        // TODO
        return AreaTheme.Circuit;
    }

    public override RoomGrid GenerateRoomGrid(Location targetLocation)
    {
        RoomGrid grid = new RoomGrid(StageManager.RoomGridDimensions);
        AreaTheme theme = GetAreaTheme(targetLocation);
        grid.AreaTheme = theme;
        Queue<Location> locations = new Queue<Location>();

        // Create location for previous room
        Location backLocation = this.GetBackLocation(targetLocation);
        if (backLocation != null)
        {
            locations.Enqueue(backLocation);
        }

        // Find all branch locations from target room
        IEnumerable<Location> branchLocations = this.GetBranchLocations(targetLocation);
        foreach (Location location in branchLocations)
        {
            locations.Enqueue(location);
        }

        // Populate and initialize list of possible rooms
        List<Room> possibleRooms = null;
        if (StageManager.SceneLoader.RoomPrefabs.Length > 0)
        {
            possibleRooms = StageManager.SceneLoader.RoomPrefabs.ToList();
        }
        else
        {
            possibleRooms = ResourceManager.GetAllRoomPrefabs(theme);
        }
        foreach (Room room in possibleRooms)
        {
            room.PopulateParts();
        }

        // Pick a starting room
        Room startingRoom = null;
        if (StageManager.SceneLoader.StartingRoomPrefabs.Length > 0)
        {
            startingRoom = StageManager.SceneLoader.StartingRoomPrefabs.GetRandom();
        }
        else
        {
            startingRoom = possibleRooms.GetRandom();
        }
        startingRoom.PopulateParts();

        Location currentLocation = null;
        List<RoomData> rooms = new List<RoomData>();
        Room currentRoom = startingRoom;
        RoomData currentRoomData = grid.AddFirstRoom(startingRoom);
        
        do
        {
            for (int i = 0; i < currentRoomData.Doors.Count; ++i)
            {
                if (locations.Count != 0)
                {
                    currentLocation = locations.Dequeue();
                    Location loc = currentLocation.Clone();
                    currentRoomData.Locations.Add(loc);
                }
            }

            if (locations.Count > 0 )
            {
                currentRoomData = grid.AddRoomFromList(possibleRooms);
                if (currentRoomData != null)
                {
                    rooms.Add(currentRoomData);
                }
            }

        } while ((locations.Count > 0) && currentRoomData != null);

        return grid;
    }
}

