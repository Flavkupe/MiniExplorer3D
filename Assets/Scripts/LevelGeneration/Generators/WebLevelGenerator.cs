using Assets.Scripts.LevelGeneration;
using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

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
            location.Name = this.HtmlDecode(titleNode.InnerText);
            if (StageManager.CurrentLocation.Path == location.Path)
            {
                StageManager.CurrentLocation.Name = location.Name;
            }
        }

        HtmlNode contentNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='mw-content-text']//div[contains(@class, 'mw-parser-output')]");
        if (contentNode == null)
        {
            Debug.LogWarning("Content node not found in Wikipedia page.");
            return;
        }

        HtmlNodeCollection subCategories = contentNode.SelectNodes("h1 | h2 | h3 | p | .//div/div[@class='thumbinner'] | .//table | .//div[@id='toc']");
        if (subCategories == null)
        {
            Debug.LogWarning("No subcategories found in content node.");
            return;
        }

        SubLocation rootSublocation = null;
        Location activeSublocation = null;
        location.LocationData.Clear();
        activeSublocation = location;

        foreach (HtmlNode node in subCategories)
        {
            switch (node.Name)
            {
                case "h2":
                    rootSublocation = ParseH2Node(node, location, location.LocationData.RawData);
                    activeSublocation = rootSublocation;
                    break;
                case "h3":
                    activeSublocation = ParseH3Node(node, location, rootSublocation, location.LocationData.RawData);
                    break;
                case "p":
                    ParseParagraphNode(node, activeSublocation, currentUri);
                    break;
                case "a":
                    ParseAnchorNode(node, activeSublocation, currentUri);
                    break;
                case "div":
                    ParseDivNode(node, activeSublocation, currentUri);
                    break;
                case "table":
                    ParseTableNode(node, activeSublocation, currentUri);
                    break;
            }
        }
    }

    private SubLocation ParseH2Node(HtmlNode node, MainLocation location, string rawData)
    {
        HtmlNode headline = node.SelectSingleNode("span[@class='mw-headline']");
        if (headline == null) return null;
        string title = this.HtmlDecode(headline.InnerText);
        var subLocation = new SubLocation(location, title)
        {
            Anchor = headline.GetAttributeValue("id", ""),
        };
        subLocation.LocationData.RawData = rawData; // TEMP
        location.LocationData.SubLocations.Add(subLocation);
        return subLocation;
    }

    private SubLocation ParseH3Node(HtmlNode node, MainLocation location, SubLocation rootSublocation, string rawData)
    {
        if (rootSublocation == null) return null;
        HtmlNode headline = node.SelectSingleNode("span[@class='mw-headline']");
        if (headline == null) return rootSublocation;
        string title = this.HtmlDecode(headline.InnerText);
        var subsubLocation = new SubLocation(location, title)
        {
            Anchor = headline.GetAttributeValue("id", ""),
        };
        subsubLocation.LocationData.RawData = rawData; // TEMP
        rootSublocation.LocationData.SubLocations.Add(subsubLocation);
        return subsubLocation;
    }

    private void ParseParagraphNode(HtmlNode node, Location activeSublocation, Uri currentUri)
    {
        string text = this.HtmlDecode(node.InnerText);
        var activeTextData = new LocationTextData(text);
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

    private void ParseAnchorNode(HtmlNode node, Location activeSublocation, Uri currentUri)
    {
        string href = node.GetAttributeValue("href", "");
        string title = node.GetAttributeValue("title", "");
        string url = "http://" + currentUri.Host + "/" + href.TrimStart('/');
        activeSublocation.LocationData.LinkedLocationData.Add(new LinkedLocationData(title, url));
    }

    private void ParseDivNode(HtmlNode node, Location activeSublocation, Uri currentUri)
    {
        if (node.GetAttributeValue("class", "") == "thumbinner")
        {
            HtmlNode imgTag = node.SelectSingleNode("a/img");
            HtmlNode caption = node.SelectSingleNode("div[@class='thumbcaption']");
            if (caption == null || imgTag == null)
            {
                return;
            }

            string imageCaption = this.HtmlDecode(caption.InnerText);
            string imageUrl = GetImageUrlFromImageTag(imgTag, currentUri.Host);
            activeSublocation.LocationData.ImagePaths.Add(new ImagePathData(imageCaption, imageUrl));
        }
        else if (node.GetAttributeValue("id", "") == "toc")
        {
            this.ParseTocNode(activeSublocation, node);
        }
    }

    private void ParseTableNode(HtmlNode node, Location activeSublocation, Uri currentUri)
    {
        string className = node.GetAttributeValue("class", "");
        if (className != null && className.Contains("infobox"))
        {
            this.ParseInfobox(currentUri, activeSublocation, node);
        }
    }

    private void ParseInfobox(Uri currentUri, Location location, HtmlNode node)
    {
        HtmlNode colspanNode = node.SelectSingleNode(".//th[@colspan] | .//td[@colspan]");
        if (colspanNode == null)
        {
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
                    string imageUrl = GetImageUrlFromImageTag(imgNode, currentUri.Host);
                    string imageCaption = this.HtmlDecode((row.InnerText ?? string.Empty).Trim());
                    location.LocationData.PodiumImages.Add(new ImagePathData(imageCaption, imageUrl));                    
                }
                else
                {
                    if (row.SelectSingleNode(".//table") != null)
                    {
                        continue;
                    }

                    HtmlNodeCollection cells = row.SelectNodes("td | th");
                    if (cells == null)
                    {
                        continue;
                    }
                    else if (cells.Count == 1 && cells[0].Name == "th")
                    {
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
        List<ImagePathData> imagePaths = new List<ImagePathData>();
        imagePaths.AddRange(location.LocationData.ImagePaths);
        imagePaths.AddRange(location.LocationData.PodiumImages);

        foreach (ImagePathData imageData in imagePaths)
        {
            LevelImage levelImage = new LevelImage() { Name = imageData.DisplayName };
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imageData.Path))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"Image load error: {uwr.error} for {imageData.Path}");
                    continue;
                }

                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                if (tex != null)
                {
                    levelImage.Texture2D = tex;
                    imageData.LoadedImage = levelImage;
                }
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
            using (UnityWebRequest uwr = UnityWebRequest.Get(location.Path))
            {
                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"Page load error: {uwr.error} for {location.Path}");
                    location.LocationData.RawData = string.Empty;
                }
                else
                {
                    location.LocationData.RawData = uwr.downloadHandler.text;
                }
            }
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

        Location backLocation = this.GetBackLocation(targetLocation);        
        if (backLocation != null)
        {
            reqs.Locations.Enqueue(backLocation);            
        }

        this.ProcessLocation(targetLocation);                
        reqs.Locations.EnqueueRange(targetLocation.LocationData.SubLocations);

        reqs.LocationText.EnqueueRange(targetLocation.LocationData.LocationText);
        reqs.ImagePaths.EnqueueRange(targetLocation.LocationData.ImagePaths);
        reqs.PodiumImages.EnqueueRange(targetLocation.LocationData.PodiumImages);
        reqs.TableOfContents = targetLocation.LocationData.TableOfContents;
        reqs.PodiumText.EnqueueRange(targetLocation.LocationData.InfoBoxData);

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
        RoomData currentRoomData = grid.AddFirstRoom(startingRoom);
        
        do
        {
            for (int i = 0; i < currentRoomData.Doors.Count; ++i)
            {
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