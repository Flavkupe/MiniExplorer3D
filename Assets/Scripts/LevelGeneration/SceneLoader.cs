using UnityEngine;
using System.Collections;
using Assets.Scripts.LevelGeneration;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System;

public enum GameDimensionMode
{
    TwoD,
    ThreeD,
};

public class SceneLoader : MonoBehaviour 
{
    private static SceneLoader instance;
    public static SceneLoader Instance { get { return instance; } }

    public AreaMapDrawer Minimap = null;
    public GameObject Player = null;
    public GameDimensionMode GameDimensionMode = GameDimensionMode.TwoD;
    public string InitialLocation = "C:\\test";

    private LoadingView Loading;

    public LevelGenerationMode Mode = LevelGenerationMode.File;

    private AreaGenerationReadyEventArgs delayedAreaLoadArgs = null;

    private ILevelGenerator levelGenerator = null;

    public Room[] RoomPrefabs;
    public Room[] StartingRoomPrefabs;

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

        if (this.levelGenerator.NeedsAreaGenPreparation)
        {
            this.levelGenerator.OnAreaGenReady += LevelGenerator_OnAreaGenReady;              
        }

        instance = this;
    }

    void Start()
    {
        this.Loading = StageManager.LoadingViewer;
        this.Loading.ToggleCamera(true);

        Location current = StageManager.CurrentLocation;        

        if (this.levelGenerator.NeedsAreaGenPreparation)
        {
            StartCoroutine(this.levelGenerator.PrepareAreaGeneration(current, this));
        }
        else 
        {
            StartCoroutine(this.GenerateLevel(current));
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
        Area area = Instantiate(ResourceManager.GetEmptyAreaPrefab()) as Area;
        area.name = currentLocation.Name ?? currentLocation.Path;
        area.DisplayName = area.name;

        RoomGrid grid = StageManager.GetAreaRoomGridOrNull(currentLocation);
        if (grid == null)
        {
            // Generate new map if none exists            
            grid = this.levelGenerator.GenerateRoomGrid(currentLocation);
        }

        // Populate rooms with stuff and create actual instances
        area.RoomGrid = grid;
        StageManager.CurrentArea = area;

        if (this.levelGenerator.NeedsAreaGenPreparation)
        {
            yield return this.levelGenerator.AreaPostProcessing(currentLocation, this);
        }

        Vector3? spawnPos = null;
        List<Room> instances = new List<Room>();
        foreach (RoomData roomData in grid.Rooms)
        {
            // Put each room from grid in its actual location
            Room model = this.GetRoomByPrefabID(roomData.PrefabID, area);
            Room roomInstance = Instantiate(model) as Room;            

            roomInstance.transform.parent = area.transform;
            roomInstance.transform.position = roomData.WorldCoords;

            roomInstance.GenerateRoomPartsFromRoomData(currentLocation, roomData);

            if (roomInstance.PlayerSpawn != null)
            {
                spawnPos = roomInstance.PlayerSpawn.position;
            }

            instances.Add(roomInstance);
        }

        area.Rooms = instances.ToArray();
        StageManager.KnownAreaMap[currentLocation.LocationKey] = area.RoomGrid;

        if (this.Minimap != null)
        {
            this.Minimap.RefreshMinimap();
        }

        this.Loading.ToggleCamera(false);
        this.Player.transform.gameObject.SetActive(true);
        if (spawnPos != null)
        {
            this.Player.transform.position = spawnPos.Value;
        }

        yield return null;         
    }

    public MonoBehaviour CreateDisabledInstance(MonoBehaviour model)
    {
        MonoBehaviour instance = Instantiate(model);
        instance.gameObject.SetActive(false);
        return instance;
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
}
