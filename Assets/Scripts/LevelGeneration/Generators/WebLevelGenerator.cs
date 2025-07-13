using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.LevelGeneration;

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
        List<ImagePathData> imagePaths = new ();
        
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
        if (section == null)
        {
            return;
        }

        if (section.ImagePaths != null)
        {
            imagePaths.AddRange(section.ImagePaths);
        }

        if (section.Subsections != null)
        {
            foreach (var sub in section.Subsections)
            {
                CollectImagesFromSection(sub, imagePaths);
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