
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Room : MonoBehaviour, IHasName
{   	
	private NameTag _nametag;

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

    public Transform PlayerSpawn;

	public Door[] Doors = new Door[] {};

    public SpawnPoint[] SpawnPoints = new SpawnPoint[] { }; 

    public RoomConnector[] Connectors = new RoomConnector[] { };

    public RoomImageFrame[] Paintings = new RoomImageFrame[] { };

    public Placeholder[] Reading = new Placeholder[] { };

    public Placeholder[] DisplayPodiums = new Placeholder[] { };

    public Placeholder TOCPodium = null;

    public AreaTitle AreaTitleSign = null;

    void Awake()
    {        
    }

	void Start() 
	{
		this._nametag = this.GetComponentInChildren<NameTag>();
	}

	void Update() {

	}

	public void SetName(string name) 
	{ 
		this.Data.DisplayName = name; 		

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
        List<Placeholder> allPlaceholders = this.GetComponentsInChildren<Placeholder>(true).ToList();

        this.Doors = this.transform.GetComponentsInChildren<Door>(true);
        this.Connectors = this.transform.GetComponentsInChildren<RoomConnector>(true);
        this.SpawnPoints = this.transform.GetComponentsInChildren<SpawnPoint>(true);
        this.Paintings = this.transform.GetComponentsInChildren<RoomImageFrame>(true).Where(a => a.FrameType == RoomImageFrame.ImageFrameType.Painting).ToArray();
        this.Reading = allPlaceholders.Where(a => a.PartType == Placeholder.RoomPartType.Reading).ToArray();

        this.DisplayPodiums = allPlaceholders.Where(a => a.PartType == Placeholder.RoomPartType.DisplayPodium ||
                                                         a.PartType == Placeholder.RoomPartType.TextPodium).ToArray();
        this.TOCPodium = allPlaceholders.FirstOrDefault(a => a.PartType == Placeholder.RoomPartType.TableOfContentsPodium);
        this.AreaTitleSign = this.transform.GetComponentInChildren<AreaTitle>();
    }

    public void GenerateRoomPartsFromRoomData(Location currentLocation, RoomData roomData)
    {
        // Apply title where applicable
        if (this.AreaTitleSign != null)
        {
            this.AreaTitleSign.SetTitle(currentLocation.Name);
        }

        List<Door> doors = new List<Door>();

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
                        if (instanceConnector.ShouldUseAlternativeDoor)
                        {
                            Debug.Assert(instanceConnector.DoorAlternative != null, "Door expected not null!");
                            doors.Add(instanceConnector.DoorAlternative);
                        }
                    }

                    break;
                }
            }
        }

        doors.AddRange(this.Doors);

        // Create the doors        
        foreach (Door door in doors)
        {
            if (roomData.Requirements.Locations.Count == 0)
            {
                if (door.RemoveOnUnused)
                {
                    door.gameObject.SetActive(false);
                }
            }
            else
            {
                Location currentLoc = roomData.Requirements.Locations.Dequeue();
                door.SetLocation(currentLoc);
                door.SetName(currentLoc.Name);
                if (StageManager.PreviousLocation != null && 
                    currentLoc.LocationKey == StageManager.PreviousLocation.LocationKey &&
                    StageManager.SceneLoader != null && StageManager.SceneLoader.Player != null)
                {                    
                    door.TeleportObjectToFront(StageManager.SceneLoader.Player);                    
                }
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

        // Place the images
        foreach (RoomImageFrame frame in this.Paintings)
        {
            if (roomData.Requirements.ImagePaths.Count > 0)
            {
                if (!frame.IsUsed)
                {
                    LevelImage image = roomData.Requirements.ImagePaths.Dequeue().LoadedImage;
                    frame.SetLevelImage(image);
                }
            }
            else
            {
                frame.gameObject.SetActive(false);
            }
        }

        // Place the podium images
        foreach (Placeholder podiumPlaceholder in this.DisplayPodiums)
        {                        
            if (podiumPlaceholder.PartType == Placeholder.RoomPartType.DisplayPodium && roomData.Requirements.PodiumImages.Count > 0)
            {
                DisplayPodium podium = podiumPlaceholder.GetInstance<DisplayPodium>();
                LevelImage image = roomData.Requirements.PodiumImages.Dequeue().LoadedImage;
                podium.SetImage(image);                
            }
            else if (podiumPlaceholder.PartType == Placeholder.RoomPartType.TextPodium && roomData.Requirements.PodiumText.Count > 0) 
            {
                DisplayPodium podium = podiumPlaceholder.GetInstance<DisplayPodium>();
                podium.SetText(roomData.Requirements.PodiumText.Dequeue());
            }
            else
            {
                podiumPlaceholder.gameObject.SetActive(false);
            }
        }

        if (this.TOCPodium != null)
        {
            if (roomData.Requirements.TableOfContents != null)
            {
                TableOfContentsPodium podium = this.TOCPodium.GetInstance<TableOfContentsPodium>();
                podium.SetTableOfContents(roomData.Requirements.TableOfContents);
                roomData.Requirements.TableOfContents = null;
            }
            else
            {
                this.TOCPodium.gameObject.SetActive(false);
            }
        }

        // Set the text
        foreach (Placeholder bookPlaceholder in this.Reading)
        {
            if (roomData.Requirements.LocationText.Count > 0)
            {
                ReadingContent book = bookPlaceholder.GetInstance<ReadingContent>();
                book.AddText(roomData.Requirements.LocationText.Dequeue());
            }
            else
            {
                bookPlaceholder.gameObject.SetActive(false);
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

    // Should not clone this in a deep copy!
    public Room RoomReference { get; set; }

    public string PrefabID { get; set; }

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

    private LevelGenRequirements requirements = new LevelGenRequirements();
    public LevelGenRequirements Requirements
    {
        get { return requirements; }
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
        data.spawnPoints = new List<SpawnPointData>();

        data.RoomReference = this.RoomReference;

        data.Requirements.Clone(deepCopy);

        if (deepCopy)
        {
            foreach (DoorData door in this.Doors)
            {
                data.Doors.Add(door.Clone());
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
