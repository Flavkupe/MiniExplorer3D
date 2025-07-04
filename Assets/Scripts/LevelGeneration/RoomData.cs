using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomData : IMatchesPrefab
{
    public int DimX;
    public int DimY;
    public int DimZ;
    public string DisplayName;
    public Room RoomReference { get; set; }
    public string PrefabID { get; set; }
    private List<RoomConnectorData> connectors = new List<RoomConnectorData>();
    private List<DoorData> doors = new List<DoorData>();
    private List<SpawnPointData> spawnPoints = new List<SpawnPointData>();
    private LevelGenRequirements requirements = new LevelGenRequirements();
    private List<ExhibitData> exhibitData = new List<ExhibitData>();
    public RoomData() { }
    public List<DoorData> Doors { get { return doors; } }
    public LevelGenRequirements Requirements { get { return requirements; } }
    public List<RoomConnectorData> Connectors { get { return connectors; } }
    public List<SpawnPointData> SpawnPoints { get { return spawnPoints; } }

    public List<ExhibitData> ExhibitData { get { return exhibitData; } }

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
            data.exhibitData = new List<ExhibitData>();
            foreach (var ex in this.ExhibitData)
            {
                data.ExhibitData.Add(new ExhibitData(ex.PrefabID, ex.SectionData));
            }
        }
        return data;
    }
}
