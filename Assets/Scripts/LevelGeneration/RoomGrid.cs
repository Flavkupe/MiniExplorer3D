using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.LevelGeneration
{
    [Serializable]
    public class RoomGrid
    {
        public MainLocation MainLocation { get; set; }
        public AreaTheme AreaTheme { get; set; }
        private List<RoomData> rooms = new List<RoomData>();    
        private RoomCell[,] grid = null;
        private List<OpenConnectorCell> openConnections = new List<OpenConnectorCell>();
        private int dimensions;

        public int Dimensions
        {
            get { return dimensions; }
        }

        public List<RoomData> Rooms
        {
            get { return rooms; }
        }

        public abstract class GridCell 
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        private class RoomCell : GridCell
        {
            public List<RoomConnectorData> connectors = new List<RoomConnectorData>();

            public RoomCell(RoomData room, int x, int y)
            {
                this.Room = room;
                this.X = x;
                this.Y = y;
            }

            public RoomData Room { get; set; }
            public List<RoomConnectorData> Connectors { get { return this.connectors; } } 
        }

        private class OpenConnectorCell : GridCell
        {
            public RoomCell NeighborCell { get; set; }
            public RoomConnectorData OpenConnector { get; set; }
            public OpenConnectorCell(RoomConnectorData openConnector, RoomCell neighborCell, int x, int y)
            {
                this.OpenConnector = openConnector;
                this.NeighborCell = neighborCell;
                this.X = x;
                this.Y = y;
            }
        }

        public RoomGrid(int dimensions) 
        {
            this.grid = new RoomCell[dimensions, dimensions];
            this.dimensions = dimensions;
        }

        public void AddRoom(RoomData data, int x, int y) 
        {
            this.rooms.Add(data);

            int stepX = data.DimX / StageManager.StepSize;
            int stepY;
            if (StageManager.SceneLoader.GameDimensionMode == GameDimensionMode.TwoD)
            {
                stepY = data.DimY / StageManager.StepSize;
            }
            else
            {
                stepY = data.DimZ / StageManager.StepSize;
            }

            for (int i = x; i < x + stepX; ++i)
            {
                for (int j = y; j < y + stepY; ++j)
                {
                    this.grid[i, j] = new RoomCell(data, i, j);                    
                }
            }

            foreach (RoomConnectorData connector in data.Connectors)
            {
                Vector2 connectorCoords = connector.RelativeGridCoords;
                RoomCell cellWithConnector = this.grid[x + (int)connectorCoords.x, y + (int)connectorCoords.y];
                if (cellWithConnector == null)
                {
                    Debug.LogError(string.Format("Room {0} connector {1} is off bounds: ({2},{3}) out of ({4},{5})",
                                    data.PrefabID, connector.PrefabID, connectorCoords.x, connectorCoords.y, stepX, stepY));
                }

                cellWithConnector.Connectors.Add(connector);
                Vector2 adjacentCoords = this.GetAdjacentConnectorCoords(cellWithConnector, connector);
                if (this.grid[(int)adjacentCoords.x, (int)adjacentCoords.y] == null)
                {
                    this.openConnections.Add(new OpenConnectorCell(connector, cellWithConnector, (int)adjacentCoords.x, (int)adjacentCoords.y));
                }
            }
        }

        private Vector2 GetAdjacentConnectorCoords(GridCell cell, RoomConnectorData connector)
        {
            int x = cell.X;
            int y = cell.Y;
            x += connector.Position == ConnectorPosition.Left ? -1 : connector.Position == ConnectorPosition.Right ? 1 : 0;
            y += connector.Position == ConnectorPosition.Top ? 1 : connector.Position == ConnectorPosition.Bottom ? -1 : 0;
            return new Vector2(x, y);
        }

        public bool CanAddRoom(Room room, int x, int y)
        {
            int stepX = room.Width / StageManager.StepSize;            
            int gridRoomHeight = StageManager.SceneLoader.GameDimensionMode == GameDimensionMode.TwoD ? room.Height : room.Length;
            int stepY = gridRoomHeight / StageManager.StepSize;

            for (int i = x; i < x + stepX; ++i)
            {
                for (int j = y; j < y + stepY; ++j)
                {
                    if (this.HasRoomAt(i, j) || 
                        !this.IsInBounds(i, j))
                    {
                        return false;
                    }                    
                }
            }

            return true;
        }

        public bool HasRoomAt(int x, int y)
        {
            if (!this.IsInBounds(x, y))
            {
                return false;
            }

            return this.grid[x, y] != null;            
        }

        public bool IsInBounds(int x, int y)
        {
            if (x < 0 || y < 0 ||
                x >= this.dimensions ||
                y >= this.dimensions)
            {
                return false;
            }

            return true;
        }

        public RoomData AddFirstRoom(Room room)
        {
            int center = this.dimensions / 2;
            RoomData data = this.GetRoomData(room, center, center);                                   
            this.AddRoom(data, center, center);
            return data;
        }

        public Vector3 GetWorldCoordsFromGridCoords(int x, int y, int objWidth, int objHeight, bool threeD)
        {
            // Center at 0, 0
            int center = this.dimensions / 2;            
            int xWorld = (x - center) * StageManager.StepSize;
            int yWorld = (y - center) * StageManager.StepSize;
            int depth = threeD ? 0 : 1;
            // Center the object from bottom-left to its center
            xWorld = xWorld + objWidth / 2;
            yWorld = yWorld + objHeight / 2;
            return threeD ? new Vector3(xWorld, depth, yWorld) : new Vector3(xWorld, yWorld, depth);
        }        

        public RoomData AddRoomFromList(List<Room> possibleRooms) 
        {
            Queue<OpenConnectorCell> openConnectors = new Queue<OpenConnectorCell>();
            foreach (OpenConnectorCell connector in this.openConnections)
            {
                openConnectors.Enqueue(connector);
            }        

            while (openConnectors.Count > 0)
            {
                // Get list of rooms that have a connector that can match any of the currently open connectors
                OpenConnectorCell currentOpenConnectorCell = openConnectors.Dequeue();
                List<Room> viableRooms = possibleRooms.Where(room => room.Connectors.Any(connector => connector.IsMatchingConnector(currentOpenConnectorCell.OpenConnector))).ToList();
                
                // TODO: select rooms more intellegently, based on list of room requirements
                viableRooms.Shuffle();
                foreach (Room room in viableRooms)
                {
                    // For each room, find connectors that can fit with an existing connector
                    List<RoomConnector> viableConnectors = room.Connectors.Where(connector => connector.IsMatchingConnector(currentOpenConnectorCell.OpenConnector)).ToList();
                    viableConnectors.Shuffle();
                    foreach (RoomConnector connector in viableConnectors)
                    {
                        // Find where connectors lie in the grid; offset is relative to the grid coords
                        Vector2 roomOffset = connector.GetRelativeGridCoords();
                        int roomCoordX = currentOpenConnectorCell.X - (int)roomOffset.x;
                        int roomCoordY = currentOpenConnectorCell.Y - (int)roomOffset.y;                                               

                        if (this.CanAddRoom(room, roomCoordX, roomCoordY))
                        {
                            RoomData data = this.GetRoomData(room, roomCoordX, roomCoordY);
                            this.AddRoom(data, roomCoordX, roomCoordY);                            
                            this.openConnections.Remove(currentOpenConnectorCell);
                            RoomConnectorData usedConnector = data.Connectors.FirstOrDefault(a => a.IsSamePrefab(connector));
                            System.Diagnostics.Debug.Assert(usedConnector != null, "Unmatching prefab IDs");
                            if (usedConnector != null)
                            {
                                usedConnector.Used = true;
                                currentOpenConnectorCell.OpenConnector.Used = true;
                            }
                            
                            return data;
                        }
                    }
                }
            }
            
            return null;
        }

        public RoomData GetRoomData(Room room, int x, int y)
        {            
            RoomData data = room.ToRoomData();
            data.RoomReference = room;
            bool threeD = StageManager.SceneLoader.GameDimensionMode == GameDimensionMode.ThreeD;
            int height = threeD ? room.Length : room.Height;
            
            data.WorldCoords = this.GetWorldCoordsFromGridCoords(x, y, room.Width, height, threeD);
            data.GridCoords = new Vector2(x, y);
            return data;
        }
    }  
}
