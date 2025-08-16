using Assets.Scripts.LevelGeneration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements.Experimental;

public abstract class WebLevelGenerator : BaseLevelGenerator
{
    private RoomSelector roomSelector = new RoomSelector();

    public WebLevelGenerator() { }

    protected override void ProcessLocation(Location parentLocation)
    {
        // TODO: is this check necessary?
        if (!(parentLocation is MainLocation))
        {
            return;
        }

        MainLocation location = parentLocation as MainLocation;
        this.ProcessHtmlDocument(location);
        location.LocationData.RemoveEmptySections();
    }

    protected abstract void ProcessHtmlDocument(MainLocation location);

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

        // Parse the raw HTML into sections
        this.ProcessLocation(targetLocation);

        LevelGenRequirements reqs = new WebLevelGenRequirements(targetLocation);

        Location backLocation = this.GetBackLocation(targetLocation);
        if (backLocation != null)
        {
            // door to previous location
            reqs.Locations.Enqueue(backLocation);
        }

        List<Room> allAvailableRooms = GetPossibleRooms(theme);
        allAvailableRooms.ForEach(room => room.PopulateParts());

        Room startingRoom = this.GetFirstRoom(targetLocation);

        List<RoomData> rooms = new List<RoomData>();
        var firstRoom = roomSelector.FindBestRoom(new List<Room> { startingRoom }, reqs);
        RoomData currentRoomData = grid.AddFirstRoom(firstRoom.Room);
        TransferMatchesToRoomData(firstRoom.Rating.MatchedSections, reqs, currentRoomData);

        int failsafeCount = 0;

        do
        {
            if (!reqs.AllRequirementsMet)
            {
                var possibleRooms = grid.GetPossibleRooms(allAvailableRooms, reqs);
                var bestRoom = roomSelector.FindBestRoom(possibleRooms, reqs);
                if (bestRoom == null)
                {
                    Debug.LogWarning("WebLevelGenerator: no more viable rooms exist to handle missing reqs; breaking loop.");
                    this.LogMissingReqs(reqs);
                    break;
                }

                var roomData = grid.AddRoomToGrid(bestRoom.Room);
                TransferMatchesToRoomData(bestRoom.Rating.MatchedSections, reqs, roomData);
            }

            if (failsafeCount++ > 30)
            {
                Debug.LogWarning("WebLevelGenerator: Failsafe triggered, stopping room generation to avoid infinite loop.");
                break;
            }

        } while (!reqs.AllRequirementsMet && currentRoomData != null);

        return grid;
    }

    /// <summary>
    /// Given a list of matches from room/exhibit selection, removes them from
    /// reqs and puts them into the room data to populate rooms later.
    /// </summary>
    private void TransferMatchesToRoomData(List<RatingResultMatch> matches, LevelGenRequirements reqs, RoomData roomData)
    {
        // Transfer section requirements to the room data
        foreach (var match in matches)
        {
            if (reqs.SectionData.Contains(match.SectionData))
            {
                roomData.ExhibitData.Add(new ExhibitData(match.PrefabID, match.SectionData));
                reqs.SectionData.Remove(match.SectionData);
            }
            else
            {
                Debug.LogWarning($"Section data {match.SectionData.Title} not found in requirements, cannot transfer to room data.");
            }
        }
    }

    /// <summary>
    /// Gets list of possible rooms to use for generation, either from the SceneLoader (if set) or from resources.
    /// </summary>
    /// <returns></returns>
    private static List<Room> GetPossibleRooms(AreaTheme theme)
    {
        List<Room> possibleRooms = null;
        if (StageManager.SceneLoader.RoomPrefabs.Length > 0)
        {
            possibleRooms = StageManager.SceneLoader.RoomPrefabs.ToList();
        }
        else
        {
            possibleRooms = ResourceManager.GetAllRoomPrefabs(theme);
        }

        return possibleRooms;
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