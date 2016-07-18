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
    public AreaMapDrawer Minimap = null;
    public GameObject Player = null;
    public GameDimensionMode GameDimensionMode = GameDimensionMode.TwoD;
    public string InitialLocation = "C:\\test";    

    public LevelGenerationMode Mode = LevelGenerationMode.File;

    private AreaGenerationReadyEventArgs delayedAreaLoadArgs = null;

    public Room[] RoomPrefabs;
    public Room[] StartingRoomPrefabs;

    void Awake() 
    {
        Player.transform.gameObject.SetActive(false);
        StageManager.SetLevelGenMode(this.Mode);
        StageManager.SceneLoader = this;

        if (StageManager.CurrentLocation == null)
        {
            StageManager.CurrentLocation = new MainLocation(this.InitialLocation);
        }

        if (StageManager.LevelGenerator.NeedsAreaGenPreparation)
        {
            StageManager.LevelGenerator.OnAreaGenReady += LevelGenerator_OnAreaGenReady;
        }
    }

    void Start()
    {
        Location current = StageManager.CurrentLocation;        

        if (StageManager.LevelGenerator.NeedsAreaGenPreparation)
        {
            StartCoroutine(StageManager.LevelGenerator.PrepareAreaGeneration(current, this));
        }
        else 
        {
            this.GenerateLevel(current);
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

    private void GenerateLevel(Location currentLocation)
    {
        Area area = Instantiate(ResourceManager.GetEmptyAreaPrefab()) as Area;
        area.name = currentLocation.Name ?? currentLocation.Path;
        area.DisplayName = area.name;

        RoomGrid grid = StageManager.GetAreaRoomGridOrNull(currentLocation);
        if (grid == null)
        {
            // Generate new map if none exists            
            grid = StageManager.LevelGenerator.GenerateRoomGrid(currentLocation);
        }

        // Populate rooms with stuff and create actual instances
        area.RoomGrid = grid;
        StageManager.CurrentArea = area;
        
        List<Room> instances = new List<Room>();
        foreach (RoomData roomData in grid.Rooms)
        {
            Room model = this.GetRoomByPrefabID(roomData.PrefabID, area);
            Room roomInstance = Instantiate(model) as Room;
            roomInstance.transform.parent = area.transform;
            roomInstance.transform.position = roomData.WorldCoords;

            roomInstance.GenerateRoomPartsFromRoomData(currentLocation, roomData);            

            instances.Add(roomInstance);
        }

        area.Rooms = instances.ToArray();
        StageManager.KnownAreaMap[currentLocation.LocationKey] = area.RoomGrid;

        if (this.Minimap != null)
        {
            this.Minimap.RefreshMinimap();
        }

        this.Player.transform.gameObject.SetActive(true);
    }

    
        
    void Update () 
    {
        if (this.delayedAreaLoadArgs != null)
        {
            try
            {
                this.GenerateLevel(this.delayedAreaLoadArgs.AreaLocation);
            }
            finally
            {
                this.delayedAreaLoadArgs = null;
            }
            
        }
    }
}
