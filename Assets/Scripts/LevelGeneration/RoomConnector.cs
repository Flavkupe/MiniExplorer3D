﻿using UnityEngine;
using System.Collections;
using System;

[Flags]
public enum RoomConnectorUsageMode
{    
    RemoveOnUsed = 1,   
    EncloseSurroundingWallsOnUnused = 2,
    ReplaceWithDoorOnUnused = 4,
}

public class RoomConnector : MonoBehaviour 
{    
    private Room parentRoom = null;
    public Room ParentRoom => this.GetOrFindParentRoom();

    public RoomConnectorData Data = new RoomConnectorData();

    /// <summary>
    /// Door to use if this is unused. Ignored if null.
    /// </summary>
    public Door DoorAlternative;

    public RoomConnectorUsageMode ConnectedUsageBehavior;

    public ClosingWall[] EnclosingWalls;

    // Use this for initialization
    void Start () {
        
    }
    
    // Update is called once per frame
    void Update () {
    
    }

    private Room FindParentRoom()
    {
        Transform t = this.transform.parent;
        while (t != null)
        {
            Room room = t.GetComponent<Room>();
            if (room != null)
                return room;
            t = t.parent;
        }
        return null;
    }

    private Room GetOrFindParentRoom()
    {
        if (this.parentRoom == null)
        {
            this.parentRoom = FindParentRoom();
            if (this.parentRoom == null)
            {
                Debug.LogError("RoomConnector could not find ParentRoom in parent hierarchy.");
            }
        }
        return this.parentRoom;
    }

    private Vector3 GetLocalPosRelativeToParentRoom()     {
        if (this.ParentRoom == null)
        {
            Debug.LogError("RoomConnector has no ParentRoom set.");
            return Vector3.zero;
        }
        return this.ParentRoom.transform.InverseTransformPoint(this.transform.position);
    }

    private ConnectorPosition DetectRelativePosition()
    {
        // Get local position relative to room center
        Vector3 localPos = GetLocalPosRelativeToParentRoom();
        float x = localPos.x;
        float y = localPos.y;
        float z = localPos.z;

        float absX = Mathf.Abs(x);
        float absZ = Mathf.Abs(z);

        if (absX > absZ)
        {
            return x > 0 ? ConnectorPosition.Right : ConnectorPosition.Left;
        }
        else
        {
            return z > 0 ? ConnectorPosition.Top : ConnectorPosition.Bottom;
        }
    }

    public string PrefabID => this.name;

    public ConnectorType Type { get { return this.Data.Type; } }

    public ConnectorPosition Position { get { return this.Data.Position; } }    

    public RoomConnectorData ToRoomConnectorData()
    {
        // Always auto-detect position before returning data
        RoomConnectorData data = this.Data.Clone();
        data.PrefabID = this.name;
        data.Position = DetectRelativePosition();
        data.RelativeGridCoords = this.GetRelativeGridCoords();
        return data;
    }

    /// <summary>
    /// Gets the grid location of this connector relative to the room and step size,
    /// where the top left is 0, 0 and the bottom right is 
    /// ((roomWidth/StageManager.StepSize)-1), ((roomHeight/StageManager.StepSize)-1)
    /// </summary>
    /// <returns></returns>
    public Vector2 GetRelativeGridCoords()
    {
        Vector3 localPos = GetLocalPosRelativeToParentRoom();
        Func<float,int,int> GridCoordAdjustment = (float coordLocal, int localDimension) =>
        {
            coordLocal += coordLocal < 0 ? 0.01f : -0.01f; // adjust for exactly 8 or -8
            int loc = (int)coordLocal + localDimension / 2;
            loc = loc / StageManager.StepSize;
            return loc;
        };

        int locX = GridCoordAdjustment(localPos.x, ParentRoom.Width);
        int locY = GridCoordAdjustment(localPos.z, ParentRoom.Length);                         

        return new Vector2(locX, locY);
    }

    public bool IsMatchingConnector(RoomConnectorData other)
    {
        var thisData = this.ToRoomConnectorData();
        return thisData.IsMatchingConnector(other);
    }

    public bool IsMatchingConnector(RoomConnector other)
    {
        var thisData = this.ToRoomConnectorData();
        var otherData = other.ToRoomConnectorData();

        return thisData.IsMatchingConnector(otherData);
    }

    public void SetUsed()
    {
        if ((this.ConnectedUsageBehavior & RoomConnectorUsageMode.RemoveOnUsed) != 0)
        {
            this.gameObject.SetActive(false);
        }
    }

    public void SetUnused()
    {

        if (this.ConnectedUsageBehavior.HasFlag(RoomConnectorUsageMode.EncloseSurroundingWallsOnUnused))
        {
            foreach (ClosingWall wall in this.EnclosingWalls)
            {
                wall.SwitchStance(true);
                this.gameObject.SetActive(false);
            }
        }
        else if (this.ConnectedUsageBehavior.HasFlag(RoomConnectorUsageMode.ReplaceWithDoorOnUnused) && this.DoorAlternative != null)
        {
            this.DoorAlternative.gameObject.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }
}

[Serializable]
public class RoomConnectorData : IMatchesPrefab 
{
    public string PrefabID { get; set; }

    public bool Used;

    public ConnectorType Type;

    public ConnectorPosition Position;

    public Vector2 RelativeGridCoords { get; set; }

    public RoomConnectorData Clone()
    {
        RoomConnectorData data = this.MemberwiseClone() as RoomConnectorData;
        data.PrefabID = this.PrefabID;
        return data;
    }

    public bool IsCloseTo(RoomConnectorData other)
    {
        return this.RelativeGridCoords.IsCloseTo(other.RelativeGridCoords);
    }

    public bool IsCloseTo(RoomConnector obj)
    {
        var data = obj.ToRoomConnectorData();
        return this.IsCloseTo(data);
    }

    public bool IsMatchingConnector(RoomConnectorData other)
    {
        if (this.Type == other.Type)
        {
            if (this.Position == ConnectorPosition.Bottom && other.Position == ConnectorPosition.Top ||
                this.Position == ConnectorPosition.Top && other.Position == ConnectorPosition.Bottom ||
                this.Position == ConnectorPosition.Right && other.Position == ConnectorPosition.Left ||
                this.Position == ConnectorPosition.Left && other.Position == ConnectorPosition.Right)
            {
                return true;
            }
        }

        return false;
    }
}

public enum ConnectorType 
{
    SmallDoor,    
}

/// <summary>
/// Position of the door relative to top-down z/x coordniate plane, with z to the "north"
/// </summary>
public enum ConnectorPosition
{
    Top,
    Bottom,
    Left, 
    Right
}