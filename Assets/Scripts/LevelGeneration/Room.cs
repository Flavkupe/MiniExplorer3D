
using System;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour, IHasName
{   	
	private NameTag nametag;

    /// <summary>
    /// X dimension
    /// </summary>
    public int Width { get { return this.Data.DimX; } }

    /// <summary>
    /// Y dimension in 2D and 3D
    /// </summary>
    public int Height { get { return this.Data.DimY; } }

    /// <summary>
    /// 0 in 2D, Z dimension in 3D
    /// </summary>
    public int Length { get { return this.Data.DimZ; } }

    public RoomData Data = new RoomData();

	public Door[] Doors = new Door[] {};

    public SpawnPoint[] SpawnPoints = new SpawnPoint[] { }; 

    public RoomConnector[] Connectors = new RoomConnector[] { };

    public RoomImageFrame[] RoomImageFrames = new RoomImageFrame[] { };

    public ReadingContent[] Reading = new ReadingContent[] { };

    void Awake()
    {        
    }

	void Start() 
	{
		this.nametag = this.GetComponentInChildren<NameTag>();
	}

	void Update() {

	}

	public void SetName(string name) 
	{ 
		this.Data.DisplayName = name; 
		if (this.nametag != null) 
		{
			this.nametag.RefreshName();
		}

		foreach (Door door in this.Doors) 
		{
			door.SetName(name);
		}
	}

	public string GetName() { return this.Data.DisplayName; }

    public RoomData ToRoomData()
    {
        RoomData data = this.Data.Clone(true);
        foreach (Door door in this.Doors) 
        {
            data.Doors.Add(door.ToDoorData()); 
        }

        foreach (RoomConnector connector in this.Connectors)
        {
            data.Connectors.Add(connector.ToRoomConnectorData()); 
        }

        foreach (SpawnPoint spawn in this.SpawnPoints)
        {
            data.SpawnPoints.Add(spawn.ToSpawnPointData());
        }

        data.PrefabID = this.name;
        return data;
    }

    public void PopulateParts()
    {
        this.Doors = this.transform.GetComponentsInChildren<Door>(true);
        this.Connectors = this.transform.GetComponentsInChildren<RoomConnector>(true);
        this.SpawnPoints = this.transform.GetComponentsInChildren<SpawnPoint>(true);
        this.RoomImageFrames = this.transform.GetComponentsInChildren<RoomImageFrame>(true);
        this.Reading = this.transform.GetComponentsInChildren<ReadingContent>(true);
    }

    public void GenerateRoomPartsFromRoomData(Location currentLocation, RoomData roomData)
    {
        // Create the doors
        int locationCount = 0;
        foreach (Door door in this.Doors)
        {
            if (locationCount >= roomData.Locations.Count)
            {
                door.gameObject.SetActive(false);
            }
            else
            {
                Location currentLoc = roomData.Locations[locationCount];
                door.SetLocation(currentLoc);
                door.SetName(currentLoc.Name);
                if (StageManager.PreviousLocation != null && 
                    currentLoc.LocationKey == StageManager.PreviousLocation.LocationKey &&
                    StageManager.SceneLoader != null && StageManager.SceneLoader.Player != null)
                {
                    StageManager.SceneLoader.Player.transform.position = door.transform.position;
                }

                locationCount++;
            }
        }

        // Spawn the entities
        int entityCount = 0;
        foreach (SpawnPoint spawn in this.SpawnPoints)
        {
            if (entityCount < roomData.SpawnPoints.Count)
            {
                SpawnPointData spawnData = roomData.SpawnPoints[entityCount];
                if (!string.IsNullOrEmpty(spawnData.Entity))
                {
                    Enemy enemyModel = ResourceManager.GetRandomEnemyOfType(spawnData.EnemyTypes);

                    if (enemyModel != null)
                    {
                        Enemy enemyInstance = Instantiate(enemyModel) as Enemy;
                        enemyInstance.name = spawnData.Entity;
                        spawn.Data = spawnData;
                        enemyInstance.transform.position = spawn.transform.position;
                        entityCount++;
                    }
                }
            }

            spawn.gameObject.SetActive(false);
        }

        // TEMP
        Queue<LevelImage> levelImages = StageManager.LevelGenerator.GetLevelImages(currentLocation).ToQueue();

        // Place the images
        foreach (RoomImageFrame frame in this.RoomImageFrames)
        {
            if (!frame.IsUsed)
            {
                if (levelImages.Count == 0)
                {
                    frame.gameObject.SetActive(false);
                }
                else
                {
                    LevelImage image = levelImages.Dequeue();
                    frame.SetLevelImage(image);
                }
            }
        }

        // Toggle the connector visuals
        foreach (RoomConnectorData connectorData in roomData.Connectors)
        {
            foreach (RoomConnector instanceConnector in this.Connectors)
            {
                if (connectorData.IsSamePrefab(instanceConnector))
                {
                    if (connectorData.Used)
                    {
                        instanceConnector.SetUsed();
                    }
                    else
                    {
                        instanceConnector.SetUnused();
                    }

                    break;
                }
            }
        }

        // Set the text
        int count = 0;
        foreach (string locationText in currentLocation.LocationData.LocationText)
        {
            if (this.Reading != null && this.Reading.Length > count)
            {
                this.Reading[count].AddText(locationText);
                count++;
            }
            else
            {
                break;
            }
        }
    }
}

[Serializable]
public class RoomData : IMatchesPrefab
{    
    public int DimX;
    public int DimY;
    public int DimZ;
    public string DisplayName;
    

    public string PrefabID { get; set; }

    private List<Location> locations = new List<Location>();
    private List<RoomConnectorData> connectors = new List<RoomConnectorData>();
    private List<DoorData> doors = new List<DoorData>();
    private List<SpawnPointData> spawnPoints = new List<SpawnPointData>();      

    public RoomData() 
    {        
    }

    public List<DoorData> Doors
    {
        get { return doors; }
    }

    public List<Location> Locations
    {
        get { return locations; }
    }

    public List<RoomConnectorData> Connectors
    {
        get { return connectors; }
    }

    public List<SpawnPointData> SpawnPoints
    {
        get { return spawnPoints; }
    }
    
    public Vector3 WorldCoords { get; set; }
    public Vector2 GridCoords { get; set; }

    public RoomData Clone(bool deepCopy = true)
    {
        RoomData data = new RoomData();
        data.WorldCoords = this.WorldCoords;
        data.GridCoords = this.GridCoords;
        data.DimX = this.DimX;
        data.DimY = this.DimY;
        data.DimZ = this.DimZ;        
        data.PrefabID = this.PrefabID;
        data.doors = new List<DoorData>();
        data.connectors = new List<RoomConnectorData>();
        data.locations = new List<Location>();
        data.spawnPoints = new List<SpawnPointData>();

        if (deepCopy)
        {
            foreach (DoorData door in this.Doors)
            {
                data.Doors.Add(door.Clone());
            }

            foreach (Location location in this.Locations)
            {
                data.Locations.Add(location.Clone());
            }

            foreach (RoomConnectorData connector in this.Connectors)
            {
                data.Connectors.Add(connector.Clone());
            }

            foreach (SpawnPointData spawn in this.SpawnPoints)
            {
                data.SpawnPoints.Add(spawn.Clone());
            }
        }

        return data;
    }
}
