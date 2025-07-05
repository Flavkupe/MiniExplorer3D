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

    public Exhibit[] Exhibits = new Exhibit[] { };

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
        this.Exhibits = this.transform.GetComponentsInChildren<Exhibit>().Where(exhibit => !exhibit.SectionTypes.HasFlag(SectionType.Subsection)).ToArray();
        foreach (var exhibit in this.Exhibits)
        {
            exhibit.PopulateParts();
        }
    }

    public void GenerateRoomPartsFromRoomData(Location currentLocation, RoomData roomData)
    {
        // Apply title where applicable
        if (this.AreaTitleSign != null)
        {
            this.AreaTitleSign.SetTitle(currentLocation.Name);
        }

        foreach (var exhibit in this.Exhibits)
        {
            var matchingExhibitData = roomData.Requirements.ExhibitData.FirstOrDefault(a => a.PrefabID == exhibit.PrefabID);
            if (matchingExhibitData != null)
            {
                exhibit.PopulateExhibit(matchingExhibitData);
            }
            else
            {
                exhibit.ClearAssignment();
                exhibit.gameObject.SetActive(false);
            }
        }

        List<Door> doors = new List<Door>();

        // Toggle the connector visuals
        foreach (RoomConnectorData connectorData in roomData.Connectors)
        {
            foreach (RoomConnector instanceConnector in this.Connectors)
            {
                if (!connectorData.IsCloseTo(instanceConnector))
                {
                    continue;
                }

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
    }

    public bool HasMatchingExhibit(List<SectionData> sections)
    {
        foreach (var section in sections)
        {
            if (this.Exhibits.Any(exhibit => exhibit.CanHandleSection(section)))
            {
                return true;
            }
        }
        return false;
    }
}
