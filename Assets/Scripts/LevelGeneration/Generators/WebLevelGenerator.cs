using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.LevelGeneration;
using System.Text;
using Newtonsoft.Json.Linq;

public abstract class WebLevelGenerator : BaseLevelGenerator
{
    public WebLevelGenerator() { }

    protected override void ProcessLocation(Location parentLocation)
    {
        if (!(parentLocation is MainLocation))
        {
            return;
        }

        MainLocation location = parentLocation as MainLocation;
        Uri currentUri = new Uri(location.Path);
        this.ProcessHtmlDocument(location, currentUri);
        location.LocationData.RemoveEmptySections();
    }

    protected abstract void ProcessHtmlDocument(MainLocation location, Uri currentUri);

    protected virtual IEnumerator ProcessImages(Location location)
    {
        // Traverse SectionData for all images
        List<ImagePathData> imagePaths = new List<ImagePathData>();
        
        foreach (var section in location.LocationData.Sections)
        {
            CollectImagesFromSection(section, imagePaths);
        }

        foreach (ImagePathData imageData in imagePaths)
        {
            LevelImage levelImage = new LevelImage() { Name = imageData.DisplayName };
            string imageUrl = Utils.EnsureHttps(imageData.Path);
            byte[] cachedData;
            if (SimpleCache.TryGetCached(imageUrl, out cachedData))
            {
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(cachedData);
                levelImage.Texture2D = tex;
                imageData.LoadedImage = levelImage;
            }
            else
            {
                using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imageUrl))
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
                        SimpleCache.SaveToCache(imageUrl, uwr.downloadHandler.data);
                    }
                }
            }
        }
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
            // Extract the Wikipedia page title from the URL
            string pageTitle = null;
            try
            {
                var uri = new Uri(location.Path);
                // Wikipedia URLs are like https://en.wikipedia.org/wiki/Page_Title
                var segments = uri.Segments;
                if (segments.Length > 0)
                {
                    pageTitle = segments.Last().TrimEnd('/');
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to parse Wikipedia URL: {location.Path} ({ex.Message})");
            }

            if (string.IsNullOrEmpty(pageTitle))
            {
                Debug.LogWarning($"Could not determine Wikipedia page title from URL: {location.Path}");
                location.LocationData.RawData = string.Empty;
            }
            else
            {
                var page = UnityWebRequest.EscapeURL(pageTitle);
                string apiUrl = $"https://en.wikipedia.org/w/api.php?action=parse&page={page}&format=json&origin=*";
                
                string json = string.Empty;
                byte[] cachedData;
                if (SimpleCache.TryGetCached(apiUrl, out cachedData))
                {
                    // Parse JSON and extract HTML
                    json = Encoding.UTF8.GetString(cachedData);
                }
                else
                {
                    using (UnityWebRequest uwr = UnityWebRequest.Get(apiUrl))
                    {
                        Debug.Log($"Sending request to {apiUrl}");
                        yield return uwr.SendWebRequest();
                        if (uwr.result != UnityWebRequest.Result.Success)
                        {
                            Debug.LogWarning($"Page load error: {uwr.error} for {apiUrl}");
                            location.LocationData.RawData = string.Empty;
                        }
                        else
                        {
                            json = uwr.downloadHandler.text;
                            SimpleCache.SaveToCache(apiUrl, Encoding.UTF8.GetBytes(json));
                        }
                    }
                }

                if (json == string.Empty)
                {
                    Debug.LogWarning($"No data received from Wikipedia API for {pageTitle}");
                    location.LocationData.RawData = string.Empty;
                    yield break;
                }

                string html = ExtractHtmlFromWikipediaApiJson(json);
                string title = ExtractTitleFromWikipediaApiJson(json);
                location.LocationData.RawData = html;
                location.Name = title;
            }
        }

        this.CallOnAreaGenReady(new AreaGenerationReadyEventArgs() { AreaLocation = StageManager.CurrentLocation });
        yield return null;
    }

    // Helper to extract the HTML from the Wikipedia API JSON response
    private string ExtractHtmlFromWikipediaApiJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return string.Empty;
        try
        {
            var obj = JObject.Parse(json);
            var html = obj["parse"]?["text"]?["*"]?.ToString();
            return html ?? string.Empty;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to parse Wikipedia API JSON: {ex.Message}");
            return string.Empty;
        }
    }

    // Helper to extract the title from the Wikipedia API JSON response
    protected string ExtractTitleFromWikipediaApiJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return string.Empty;
        try
        {
            var obj = JObject.Parse(json);
            var title = obj["parse"]?["title"]?.ToString();
            return title ?? string.Empty;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to parse Wikipedia API JSON for title: {ex.Message}");
            return string.Empty;
        }
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

        this.ProcessLocation(targetLocation);

        // Use new SectionData-based requirements constructor
        LevelGenRequirements reqs = new WebLevelGenRequirements(targetLocation);

        Location backLocation = this.GetBackLocation(targetLocation);
        if (backLocation != null)
        {
            reqs.Locations.Enqueue(backLocation);
        }

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

        Room startingRoom = this.GetFirstRoom(targetLocation);

        List<RoomData> rooms = new List<RoomData>();
        RoomData currentRoomData = grid.AddFirstRoom(startingRoom, reqs);

        int failsafeCount = 0;

        do
        {
            if (!reqs.AllRequirementsMet)
            {
                currentRoomData = grid.AddRoomFromList(possibleRooms, reqs);
                if (currentRoomData != null)
                {
                    rooms.Add(currentRoomData);
                }
                else
                {
                    Debug.LogWarning("WebLevelGenerator: no more viable rooms exist to handle missing reqs; breaking loop.");
                    this.LogMissingReqs(reqs);
                    break;
                }
            }

            if (failsafeCount++ > 30)
            {
                Debug.LogWarning("WebLevelGenerator: Failsafe triggered, stopping room generation to avoid infinite loop.");
                break;
            }

        } while (!reqs.AllRequirementsMet && currentRoomData != null);

        return grid;
    }

    private void LogMissingReqs(LevelGenRequirements reqs)
    {
        if (reqs.Locations.Count > 0)
        {
            var locations = string.Join(',', reqs.Locations.Select(a => a.LocationKey));
            Debug.LogWarning($"WebLevelGenerator: Incomplete locations: {locations}");
        }

        if (reqs.SectionData.Count > 0)
        {
            var sections = string.Join(',', reqs.SectionData.Select(a => $"{a.Title} ({a.SectionType.ToString()})"));
            Debug.LogWarning($"WebLevelGenerator: Unmatched sections: {sections}");
        }
    }
}

public class WebLevelGenRequirements : LevelGenRequirements
{
    public WebLevelGenRequirements() : base() { }
    public WebLevelGenRequirements(Location location) : base(location) { }

    public override bool AllRequirementsMet
    {
        get
        {
            // The "or" is correct here; if we match all the sections we are done
            return this.Locations.Count == 0 || this.SectionData.Count == 0;
        }
    }

    protected override LevelGenRequirements GetInstance()
    {
        return new WebLevelGenRequirements();
    }
}