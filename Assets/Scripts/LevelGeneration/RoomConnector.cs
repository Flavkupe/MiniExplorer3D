using UnityEngine;
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
    /// Door to use if this is unused.
    /// </summary>
    public Door DoorAlternative;

    public bool ShouldUseAlternativeDoor = false;

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

    private void AutoDetectPosition()
    {
        // Get local position relative to room center
        Vector3 localPos = ParentRoom.transform.InverseTransformPoint(this.transform.position);
        float x = localPos.x;
        float y = localPos.y;
        float z = localPos.z;

        float absX = Mathf.Abs(x);
        float absZ = Mathf.Abs(z);

        if (absX > absZ)
        {
            Data.Position = x > 0 ? ConnectorPosition.Right : ConnectorPosition.Left;
        }
        else
        {
            Data.Position = (z > 0 ? ConnectorPosition.Top : ConnectorPosition.Bottom);
        }
    }

    public string PrefabID 
    { 
        get 
        {            
            return this.Data.PrefabID; 
        } 
    }

    public ConnectorType Type { get { return this.Data.Type; } }

    public ConnectorPosition Position { get { return this.Data.Position; } }    

    public RoomConnectorData ToRoomConnectorData()
    {
        // Always auto-detect position before returning data
        AutoDetectPosition();
        RoomConnectorData data = this.Data.Clone();
        data.PrefabID = this.name;
        data.RelativeGridCoords = this.GetRelativeGridCoords();
        data.Position = this.Data.Position; // ensure position is up to date
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
        Func<float,int,int> GridCoordAdjustment = (float coordLocal, int localDimension) =>
        {
            coordLocal += coordLocal < 0 ? 0.01f : -0.01f; // adjust for exactly 8 or -8
            int loc = (int)coordLocal + localDimension / 2;
            loc = loc / StageManager.StepSize;
            return loc;
        };

        Vector3 trueLocal = new Vector3();
        Transform current = this.transform;
        while (current != null && current.parent != null) // Until current is the room
        {            
            current = current.parent;
        }

        trueLocal = this.transform.position - current.position;
        

        int locX = GridCoordAdjustment(trueLocal.x, ParentRoom.Width);
        int locY = GridCoordAdjustment(trueLocal.z, ParentRoom.Length);                         

        return new Vector2(locX, locY);
    }    

    public bool IsMatchingConnector(RoomConnectorData other)
    {
        return this.Data.IsMatchingConnector(other);
    }

    public bool IsMatchingConnector(RoomConnector other)
    {
        return this.Data.IsMatchingConnector(other.Data);
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

        if ((this.ConnectedUsageBehavior & RoomConnectorUsageMode.EncloseSurroundingWallsOnUnused) != 0)
        {
            foreach (ClosingWall wall in this.EnclosingWalls)
            {
                wall.SwitchStance(true);
                this.gameObject.SetActive(false);
            }
        }

        if ((this.ConnectedUsageBehavior & RoomConnectorUsageMode.ReplaceWithDoorOnUnused) != 0)
        {
            this.ShouldUseAlternativeDoor = true;
            this.DoorAlternative.gameObject.SetActive(true);
            //this.gameObject.SetActive(false);
        }
    }
}

[Serializable]
public class RoomConnectorData : IMatchesPrefab 
{
    public string PrefabID { get; set; }

    public bool Used { get; set; }

    public ConnectorType Type;

    public ConnectorPosition Position;

    public Vector2 RelativeGridCoords { get; set; }

    public RoomConnectorData Clone()
    {
        RoomConnectorData data = this.MemberwiseClone() as RoomConnectorData;
        data.PrefabID = this.PrefabID;
        return data;
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