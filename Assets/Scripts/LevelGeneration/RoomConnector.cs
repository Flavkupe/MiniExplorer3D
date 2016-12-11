using UnityEngine;
using System.Collections;
using System;

[Flags]
public enum RoomConnectorUsageMode
{    
    RemoveOnUsed = 1,   
    EncloseSurroundingWallsOnUnused = 2,
}

public class RoomConnector : MonoBehaviour 
{    
    public Room ParentRoom = null;

    public RoomConnectorData Data = new RoomConnectorData();

    [EnumFlags]
    public RoomConnectorUsageMode ConnectedUsageBehavior;

    public ClosingWall[] EnclosingWalls;

    void Awake()
    {
        
    }

    // Use this for initialization
    void Start () {
        
    }
    
    // Update is called once per frame
    void Update () {
    
    }

    public string PrefabID 
    { 
        get 
        {
            //if (string.IsNullOrEmpty(this.Data.PrefabID))
            //{
            //    this.Data.PrefabID = Guid.NewGuid().ToString();
            //}
            
            return this.Data.PrefabID; 
        } 
    }

    public ConnectorType Type { get { return this.Data.Type; } }

    public ConnectorPosition Position { get { return this.Data.Position; } }    

    public RoomConnectorData ToRoomConnectorData()
    {
        RoomConnectorData data = this.Data.Clone();
        //data.PrefabID = this.PrefabID;
        data.PrefabID = this.name;
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
        // Shift location such that room's bottom-left is at 0,0, rather than center at 0,0

        Func<float,int,int> GridCoordAdjustment = (float coordLocal, int localDimension) =>
        {            
            int loc = (int)coordLocal + localDimension / 2;
            loc = loc / StageManager.StepSize;
            return loc;
        };

        // Get the localPos relative to the Room.
        //Vector3 trueLocal = new Vector3();
        //Transform current = this.transform;
        //while (current != null && current.parent != null) // Until current is the room
        //{
        //    trueLocal += current.transform.localPosition;
        //    current = current.parent;
        //}

        Vector3 trueLocal = new Vector3();
        Transform current = this.transform;
        while (current != null && current.parent != null) // Until current is the room
        {            
            current = current.parent;
        }

        trueLocal = this.transform.position - current.position;
        

        int locX = GridCoordAdjustment(trueLocal.x, this.ParentRoom.Width);
        int locY;                       
        if (StageManager.SceneLoader.GameDimensionMode == GameDimensionMode.TwoD)
        {
            locY = GridCoordAdjustment(trueLocal.y, this.ParentRoom.Height);             
        }
        else
        {
            locY = GridCoordAdjustment(trueLocal.z, this.ParentRoom.Length);                         
        }
        
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