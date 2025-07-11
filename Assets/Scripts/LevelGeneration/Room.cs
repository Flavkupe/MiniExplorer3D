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

    public SpawnPoint[] SpawnPoints = new SpawnPoint[] { }; 

    public RoomConnector[] Connectors = new RoomConnector[] { };

    public Placeholder TOCPodium = null;

    public AreaTitle AreaTitleSign = null;

    public ExhibitBase[] Exhibits = new ExhibitBase[] { };

    void Awake()
    {        
    }

	void Start() 
	{
		this._nametag = this.GetComponentInChildren<NameTag>();
	}

	void Update() {

	}

    public string Name => this.Data.DisplayName;

    public RoomData ToRoomData()
    {
        RoomData data = this.Data.Clone(true);
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

        this.Connectors = this.transform.GetComponentsInChildren<RoomConnector>(true);
        this.SpawnPoints = this.transform.GetComponentsInChildren<SpawnPoint>(true);
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
            var matchingExhibitData = roomData.ExhibitData.FirstOrDefault(a => a.PrefabID == exhibit.PrefabID);
            if (matchingExhibitData != null)
            {
                exhibit.PopulateExhibit(matchingExhibitData);
            }
            else
            {
                exhibit.ReplaceWithUnused();
            }
        }

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
                }

                break;
            }
        }

        // TODO: handle creating doors for "return" locations, and lobby
        // Create the doors        
        //foreach (Door door in doors)
        //{
        //    if (roomData.Requirements.Locations.Count == 0)
        //    {
        //        if (door.RemoveOnUnused)
        //        {
        //            door.gameObject.SetActive(false);
        //        }
        //    }
        //    else
        //    {
        //        Location currentLoc = roomData.Requirements.Locations.Dequeue();
        //        door.SetLocation(currentLoc);
        //        door.SetName(currentLoc.Name);
        //        if (StageManager.PreviousLocation != null &&
        //            currentLoc.LocationKey == StageManager.PreviousLocation.LocationKey &&
        //            StageManager.SceneLoader != null && StageManager.SceneLoader.Player != null)
        //        {
        //            door.TeleportObjectToFront(StageManager.SceneLoader.Player);
        //        }
        //    }
        //}
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

    /// <summary>
    /// Rates how well this Room matches the given LevelGenRequirements (sections only, skips TOC). Uses Exhibit.RateSectionMatch for each section.
    /// </summary>
    public RatingResult RateRequirementsMatch(LevelGenRequirements reqs)
    {
        return RatingProcessor.RateRoomMatch(this, reqs);
    }

    /// <summary>
    /// Validates that all child Exhibits and Connectors have unique names. Logs warnings if duplicates are found.
    /// </summary>
    public void ValidateUniqueNames()
    {
        // Validate Exhibits
        var exhibits = this.GetComponentsInChildren<Exhibit>(true);
        var exhibitGroups = exhibits.GroupBy(e => e.PrefabID).Where(g => g.Count() > 1);
        foreach (var group in exhibitGroups)
        {
            Debug.LogWarning($"Room '{this.name}': Multiple Exhibit objects found with the name '{group.Key}'. Count: {group.Count()}");
        }

        // Validate Connectors
        var connectors = this.GetComponentsInChildren<RoomConnector>(true);
        var connectorGroups = connectors.GroupBy(c => c.PrefabID).Where(g => g.Count() > 1);
        foreach (var group in connectorGroups)
        {
            Debug.LogWarning($"Room '{this.name}': Multiple RoomConnector objects found with the name '{group.Key}'. Count: {group.Count()}");
        }
    }

    void OnValidate()
    {
        ValidateUniqueNames();
    }
}
