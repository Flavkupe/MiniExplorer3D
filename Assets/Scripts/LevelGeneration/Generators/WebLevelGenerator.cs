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
    public WebLevelGenerator()
    {
    }

    protected override void ProcessLocation(Location parentLocation)
    {               
        if (!(parentLocation is MainLocation))
        {
            // If this is not a MainLocation, we've already processed it            
            return;
        }

        MainLocation location = parentLocation as MainLocation;
        Uri currentUri = new Uri(location.Path);

        this.ProcessHtmlDocument(location, currentUri);
    }

    protected virtual void ProcessHtmlDocument(MainLocation location, Uri currentUri)
    {
        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(location.LocationData.RawData);
        
        HtmlNode titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1");
        if (titleNode != null)
        {
            // This is the actual location title
            location.Name = this.HtmlDecode(titleNode.InnerText);
            if (StageManager.CurrentLocation.Path == location.Path)
            {
                StageManager.CurrentLocation.Name = location.Name;
            }
        }

        HtmlNode contentNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='mw-content-text']");

        HtmlNodeCollection subCategories = contentNode.SelectNodes("h1 | h2 | h3 | p | p/a | .//div/div[@class='thumbinner'] | .//table | .//div[@id='toc']");
        
        SubLocation rootSublocation = null;
        Location activeSublocation = null;
        LocationTextData activeTextData = null;
        location.LocationData.Clear();
        activeSublocation = location;
        foreach (HtmlNode node in subCategories)
        {
            if (node.Name == "h2")
            {
                HtmlNode headline = node.SelectSingleNode("span[@class='mw-headline']");
                string title = this.HtmlDecode(headline.InnerText);
                rootSublocation = new SubLocation(location, title);
                rootSublocation.Anchor = headline.GetAttributeValue("id", "");
                rootSublocation.LocationData.RawData = location.LocationData.RawData; // TEMP
                location.LocationData.SubLocations.Add(rootSublocation);
                activeSublocation = rootSublocation;
            }
            else if (node.Name == "h3")
            {
                HtmlNode headline = node.SelectSingleNode("span[@class='mw-headline']");
                string title = this.HtmlDecode(headline.InnerText);
                SubLocation subsubLocation = new SubLocation(location, title);
                subsubLocation.Anchor = headline.GetAttributeValue("id", "");
                subsubLocation.LocationData.RawData = location.LocationData.RawData; // TEMP
                rootSublocation.LocationData.SubLocations.Add(subsubLocation);
                activeSublocation = subsubLocation;
            }
            else if (node.Name == "p")
            {
                string text = this.HtmlDecode(node.InnerText);
                activeTextData = new LocationTextData(text);
                HtmlNodeCollection linkNodes = node.SelectNodes("a");
                if (linkNodes != null)
                {
                    foreach (HtmlNode link in linkNodes)
                    {
                        string name = this.HtmlDecode(link.InnerText);
                        string href = link.GetAttributeValue("href", "");
                        string url = "http://" + currentUri.Host + "/" + href.TrimStart('/');
                        activeTextData.LinkedLocationData.Add(new LinkedLocationData(name, url));
                    }
                }

                activeSublocation.LocationData.LocationText.Add(activeTextData);
            }
            else if (node.Name == "a")
            {
                // anchor nodes casually strewn about; these are random linked locations.                                
                string href = node.GetAttributeValue("href", "");
                string title = node.GetAttributeValue("title", "");
                string url = "http://" + currentUri.Host + "/" + href.TrimStart('/');
                activeSublocation.LocationData.LinkedLocationData.Add(new LinkedLocationData(title, url));
            }
            else if (node.Name == "div")
            {
                if (node.GetAttributeValue("class", "") == "thumbinner")
                {
                    // These are embedded thumbnails
                    HtmlNode imgTag = node.SelectSingleNode("a/img");
                    HtmlNode caption = node.SelectSingleNode("div[@class='thumbcaption']");
                    string imageCaption = this.HtmlDecode(caption.InnerText);
                    string imageUrl = GetImageUrlFromImageTag(imgTag, currentUri.Host);
                    activeSublocation.LocationData.ImagePaths.Add(new ImagePathData(imageCaption, imageUrl));
                }
                else if (node.GetAttributeValue("id", "") == "toc")
                {
                    this.ParseTocNode(activeSublocation, node);
                }
            }
            else if (node.Name == "table")
            {
                string className = node.GetAttributeValue("class", "");
                if (className != null && className.Contains("infobox"))
                {
                    this.ParseInfobox(currentUri, activeSublocation, node);
                }
            }
        }
    }

    private void ParseInfobox(Uri currentUri, Location location, HtmlNode node)
    {
        // Get the number of columns in this table
        HtmlNode colspanNode = node.SelectSingleNode(".//th[@colspan] | .//td[@colspan]");
        if (colspanNode == null)
        {
            // Ignore this one
            return;            
        }
        int colspan;
        if (!int.TryParse(colspanNode.GetAttributeValue("colspan", ""), out colspan))
        {
            colspan = 1;
        }

        HtmlNodeCollection rows = node.SelectNodes(".//tr");
        
        if (rows != null && rows.Count > 0)
        {
            InfoBoxData data = null;
            foreach (HtmlNode row in rows)
            {
                HtmlNode imgNode = row.SelectSingleNode(".//a[@class='image']/img");
                if (imgNode != null)
                {
                    // Row is for an image                    
                    string imageUrl = GetImageUrlFromImageTag(imgNode, currentUri.Host);
                    string imageCaption = this.HtmlDecode((row.InnerText ?? string.Empty).Trim());
                    location.LocationData.PodiumImages.Add(new ImagePathData(imageCaption, imageUrl));                    
                }
                else
                {
                    if (row.SelectSingleNode(".//table") != null)
                    {
                        // Beware inner tables; ignore them for now
                        continue;
                    }

                    // Text or section
                    HtmlNodeCollection cells = row.SelectNodes("td | th");
                    if (cells == null)
                    {
                        continue;
                    }
                    else if (cells.Count == 1 && cells[0].Name == "th")
                    {
                        // Title row
                        data = new InfoBoxData(colspan);
                        data.SectionTitle = this.HtmlDecode(cells[0].InnerText);
                        location.LocationData.InfoBoxData.Add(data);
                    }
                    else if (data != null)
                    {                        
                        string[] cellData = new string[colspan];
                        for (int i = 0; i < cells.Count && i < colspan; i++)
                        {
                            cellData[i] = this.HtmlDecode(cells[i].InnerText);
                        }                        

                        // make sure to ensure at least one item exists first
                        if (!cellData.ToList().All(a => string.IsNullOrEmpty(a)))
                        {
                            data.Rows.Add(cellData);
                        }
                    }
                }
            }
        }
    }

    private string HtmlDecode(string text)
    {
        return string.IsNullOrEmpty(text) ? string.Empty : HtmlEntity.DeEntitize(text);
    }

    private void ParseTocNode(Location activeSublocation, HtmlNode node)
    {
        // Table of contents
        HtmlNodeCollection items = node.SelectNodes(".//li/a");
        foreach (HtmlNode item in items)
        {
            string anchor = item.GetAttributeValue("href", "");
            anchor = (anchor ?? "").Trim('#');

            HtmlNodeCollection spans = item.SelectNodes("span");
            if (spans != null && spans.Count == 2)
            {
                if (activeSublocation.LocationData.TableOfContents == null)
                {
                    activeSublocation.LocationData.TableOfContents = new TableOfContents();
                }

                string rank = spans[0].InnerText;
                string name = spans[1].InnerText;
                activeSublocation.LocationData.TableOfContents.TocItems.Add(new TableOfContents.TOCItem(name, rank, anchor, rank.Contains(".") ? 1 : 0));
            }
        }
    }

    private string GetImageUrlFromImageTag(HtmlNode node, string currentUriHost)
    {
        string imageSrc = node.GetAttributeValue("src", "");                
        if (imageSrc.StartsWith("//"))
        {
            return "http://" + imageSrc.TrimStart('/');
        }
        else
        {
            return "http://" + currentUriHost + "/" + imageSrc.TrimStart('/');
        }
    }

    protected virtual IEnumerator ProcessImages(Location location)
    {       
        Uri currentUri = new Uri(location.Path);

        List<ImagePathData> imagePaths = new List<ImagePathData>();
        imagePaths.AddRange(location.LocationData.ImagePaths);
        imagePaths.AddRange(location.LocationData.PodiumImages);

        foreach (ImagePathData imageData in imagePaths)
        {
            LevelImage levelImage = new LevelImage() { Name = imageData.DisplayName };
            WWW www = new WWW(imageData.Path);
            yield return www;

            if (www.error != null)
            {
                continue;
            }

            if (www.texture != null)
            {
                levelImage.Texture2D = new Texture2D(www.texture.width, www.texture.height);

                www.LoadImageIntoTexture(levelImage.Texture2D);

                imageData.LoadedImage = levelImage;
            }
        }        
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

        this.CallOnAreaGenReady(new AreaGenerationReadyEventArgs() { AreaLocation = StageManager.CurrentLocation });
        yield return null;
    }

    public override IEnumerator AreaPostProcessing(Location location, MonoBehaviour caller)
    {                
        yield return caller.StartCoroutine(this.ProcessImages(location));
        this.CallOnAreaPostProcessingDone(new AreaGenerationReadyEventArgs() { AreaLocation = StageManager.CurrentLocation });
        yield return null;
    }

    public override bool NeedsAreaGenPreparation { get { return true; } }

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

        LevelGenRequirements reqs = new WebLevelGenRequirements();        

        // Create location for previous room
        Location backLocation = this.GetBackLocation(targetLocation);        
        if (backLocation != null)
        {
            reqs.Locations.Enqueue(backLocation);            
        }

        // Find all branch locations and other things in target room
        this.ProcessLocation(targetLocation);                
        reqs.Locations.EnqueueRange(targetLocation.LocationData.SubLocations);

        // Get enqueue the text from the location as a requirement 
        reqs.LocationText.EnqueueRange(targetLocation.LocationData.LocationText);
        reqs.ImagePaths.EnqueueRange(targetLocation.LocationData.ImagePaths);
        reqs.PodiumImages.EnqueueRange(targetLocation.LocationData.PodiumImages);
        reqs.TableOfContents = targetLocation.LocationData.TableOfContents;
        reqs.PodiumText.EnqueueRange(targetLocation.LocationData.InfoBoxData);

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
        possibleRooms.ForEach(room => room.PopulateParts());        

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
                // Associate new locations with available doors
                if (reqs.Locations.Count > 0)
                {
                    currentLocation = reqs.Locations.Dequeue();
                    Location loc = currentLocation.Clone();
                    currentRoomData.Requirements.Locations.Enqueue(loc);
                }
            }

            foreach (RoomImageFrame frame in currentRoomData.RoomReference.Paintings)
            {
                if (reqs.ImagePaths.Count > 0)
                {
                    currentRoomData.Requirements.ImagePaths.TransferOneFrom(reqs.ImagePaths);
                }
            }

            foreach (Placeholder podium in currentRoomData.RoomReference.DisplayPodiums)
            {
                if (podium.PartType == Placeholder.RoomPartType.DisplayPodium && reqs.PodiumImages.Count > 0)
                {
                    currentRoomData.Requirements.PodiumImages.TransferOneFrom(reqs.PodiumImages);
                }
                else if (podium.PartType == Placeholder.RoomPartType.TextPodium && reqs.PodiumText.Count > 0)
                {
                    currentRoomData.Requirements.PodiumText.TransferOneFrom(reqs.PodiumText);
                }
            }

            foreach (Placeholder book in currentRoomData.RoomReference.Reading)
            {
                if (reqs.LocationText.Count > 0)
                {
                    currentRoomData.Requirements.LocationText.TransferOneFrom(reqs.LocationText);
                }
            }

            if (currentRoomData.RoomReference.TOCPodium != null)
            {
                currentRoomData.Requirements.TableOfContents = reqs.TableOfContents;
                reqs.TableOfContents = null;
            }

            if (!reqs.AllRequirementsMet)
            {
                // If there are still more requirements left over, create a new room.
                currentRoomData = grid.AddRoomFromList(possibleRooms);
                if (currentRoomData != null)
                {
                    rooms.Add(currentRoomData);
                }
            }

        } while (!reqs.AllRequirementsMet && currentRoomData != null);

        return grid;
    }
}

public class WebLevelGenRequirements : LevelGenRequirements
{
    public override bool AllRequirementsMet
    {
        get
        {
            return this.Locations.Count == 0 && this.LocationText.Count == 0 && this.ImagePaths.Count == 0;
        }
    }    

    protected override LevelGenRequirements GetInstance()
    {
        return new WebLevelGenRequirements();
    }
}