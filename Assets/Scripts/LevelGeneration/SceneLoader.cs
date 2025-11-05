using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public enum GameDimensionMode
{
    ThreeD,
};

public class SceneLoader : MonoBehaviour 
{
    private static SceneLoader instance;
    public static SceneLoader Instance { get { return instance; } }

    public AreaMapDrawer Minimap = null;
    public GameObject Player = null;
    public GameDimensionMode GameDimensionMode = GameDimensionMode.ThreeD;
    public string InitialLocation = "C:\\test";

    private LoadingView Loading;

    public LevelGenerationMode Mode = LevelGenerationMode.Wikipedia;

    private AreaGenerationReadyEventArgs delayedAreaLoadArgs = null;

    private ILevelGenerator levelGenerator = null;

    public Room[] RoomPrefabs;
    public Room[] StartingRoomPrefabs;
    public Room[] EntranceRoomPrefabs;

    void Awake() 
    {        
        Player.transform.gameObject.SetActive(false);
        StageManager.SetLevelGenMode(this.Mode);
        StageManager.SceneLoader = this;

        this.levelGenerator = StageManager.LevelGenerator;

        if (StageManager.CurrentLocation == null)
        {
            StageManager.CurrentLocation = new MainLocation(this.InitialLocation);
        }

        this.levelGenerator.OnAreaGenReady += LevelGenerator_OnAreaGenReady;

        instance = this;
    }

    void Start()
    {
        this.Loading = StageManager.LoadingViewer;
        this.Loading.ToggleCamera(true);
        LoadLocation(StageManager.CurrentLocation);
    }

    private void LoadLocation(Location location)
    {
        if (location == null)
        {
            Debug.LogError("Cannot load a null location.");
            return;
        }

        if (location.IsRandomLocation)
        {
            LoadRandomArticle();
        }
        else if (location.IsEmptyLocation)
        {
            LoadMainPage();
        }
        else
        {
            StartCoroutine(this.levelGenerator.PrepareAreaGeneration(location, this));
        }
        
    }

    void LevelGenerator_OnAreaGenReady(object sender, AreaGenerationReadyEventArgs e)
    {
        this.delayedAreaLoadArgs = e;
    }

    private Room GetRoomByPrefabID(string id, Area area)
    {
        List<Room> linked = new List<Room>();
        linked.AddRange(this.StartingRoomPrefabs);
        linked.AddRange(this.EntranceRoomPrefabs);
        linked.AddRange(this.RoomPrefabs);

        Room room = linked.FirstOrDefault(a => string.Equals(a.name, id, System.StringComparison.OrdinalIgnoreCase));
        if (room == null)
        {
            return ResourceManager.GetRoomByPrefabID(area.Theme, id);
        }

        if (room == null)
        {
            throw new Exception("No room found with id " + id);
        }

        return room;
    }

    private IEnumerator GenerateLevel(Location currentLocation)
    {
        // 1. Open the Loading UI
        if (this.Loading != null)
        {
            this.Loading.ToggleCamera(true);
        }

        // 2. Delete the current area
        if (StageManager.CurrentArea != null)
        {
            Destroy(StageManager.CurrentArea.gameObject);
            StageManager.CurrentArea = null;
        }

        // 3. Generate the new area as normal
        Area area = Instantiate(ResourceManager.GetEmptyAreaPrefab()) as Area;
        area.name = currentLocation.Name ?? currentLocation.Path;
        area.DisplayName = area.name;

        // Populate rooms with stuff and create actual instances
        area.RoomGrid = this.levelGenerator.GenerateRoomGrid(currentLocation); ;
        StageManager.CurrentArea = area;

        yield return this.levelGenerator.AreaPostProcessing(currentLocation, this);

        Vector3? spawnPos = null;
        Quaternion? spawnRotation = null;
        List<Room> instances = new List<Room>();
        foreach (RoomData roomData in area.RoomGrid.Rooms)
        {
            // Put each room from grid in its actual location
            Room model = this.GetRoomByPrefabID(roomData.PrefabID, area);
            Room roomInstance = Instantiate(model);

            roomInstance.transform.parent = area.transform;
            roomInstance.transform.position = roomData.WorldCoords;

            roomInstance.GenerateRoomPartsFromRoomData(currentLocation, roomData);

            if (roomInstance.PlayerSpawn != null)
            {
                spawnPos = roomInstance.PlayerSpawn.position;
                spawnRotation = roomInstance.PlayerSpawn.rotation;
            }

            instances.Add(roomInstance);
        }

        area.Rooms = instances.ToArray();

        if (this.Minimap != null)
        {
            this.Minimap.RefreshMinimap();
        }

        this.Loading.ToggleCamera(false);
        this.Player.transform.gameObject.SetActive(true);
        if (spawnPos != null)
        {
            this.Player.transform.position = spawnPos.Value;
            this.Player.transform.rotation = spawnRotation ?? Quaternion.identity;
        }

        yield return null;         
    }

    public MonoBehaviour CreateDisabledInstance(MonoBehaviour model)
    {
        MonoBehaviour instance = Instantiate(model);
        instance.gameObject.SetActive(false);
        return instance;
    }

    /// <summary>
    /// Loads a random article. This can also be called from the UI
    /// through an inspector event.
    /// </summary>
    public void LoadRandomArticle()
    {
        this.Loading = StageManager.LoadingViewer;
        this.Loading.ToggleCamera(true);
        WindowManager.Instance.CloseAllWindows();
        StartCoroutine(this.levelGenerator.GenerateRandom(this));
    }

    public void LoadMainPage()
    {
        LoadWikipediaArticle(WikipediaConstants.MainPageName);
    }

    public void LoadWikipediaArticle(string articleName)
    {
        if (string.IsNullOrEmpty(articleName))
        {
            return;
        }

        // Wikipedia expects underscores for spaces
        string safeName = articleName.Replace(' ', '_');
        // Only use the article name as the path, not a full URL
        StageManager.CurrentLocation = new MainLocation(safeName, articleName);

        // Start the area generation process
        LoadLocation(StageManager.CurrentLocation);
    }

    void Update () 
    {
        if (this.delayedAreaLoadArgs != null)
        {
            try
            {
                StartCoroutine(GenerateLevel(this.delayedAreaLoadArgs.AreaLocation));
            }
            finally
            {
                this.delayedAreaLoadArgs = null;
            }
        }        
    }

    [ContextMenu("Clear cache")]
    public void ClearCache()
    {
        string cachePath = Application.temporaryCachePath;
        Debug.Log($"Clearing cache at: {cachePath}");
        try
        {
            if (System.IO.Directory.Exists(cachePath))
            {
                // Delete all files
                foreach (var file in System.IO.Directory.GetFiles(cachePath))
                {
                    System.IO.File.Delete(file);
                }
                // Delete all subdirectories
                foreach (var dir in System.IO.Directory.GetDirectories(cachePath))
                {
                    System.IO.Directory.Delete(dir, true);
                }
                Debug.Log("Cache cleared.");
            }
            else
            {
                Debug.LogWarning($"Cache path does not exist: {cachePath}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error clearing cache: {ex.Message}");
        }
    }
}
