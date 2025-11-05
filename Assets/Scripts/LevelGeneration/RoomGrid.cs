using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Android;

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

        private void PlaceRoomAtCoordinate(RoomData data, int x, int y) 
        {
            this.rooms.Add(data);

            int stepX = data.DimX / StageManager.StepSize;
            int stepY = data.DimZ / StageManager.StepSize;

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
                    continue;
                }

                cellWithConnector.Connectors.Add(connector);
                Vector2 adjacentCoords = this.GetAdjacentConnectorCoords(cellWithConnector, connector);
                var adjacentCell = this.grid[(int)adjacentCoords.x, (int)adjacentCoords.y];
                if (adjacentCell == null)
                {
                    this.openConnections.Add(new OpenConnectorCell(connector, cellWithConnector, (int)adjacentCoords.x, (int)adjacentCoords.y));
                }
                else
                {
                    // look for matching connector in the adjacent cell that hasn't been used yet
                    var existingConnector = adjacentCell.Connectors.FirstOrDefault(a => a.IsMatchingConnector(connector));
                    if (existingConnector != null)
                    {
                        existingConnector.Used = true;
                        connector.Used = true;
                    }
                    else
                    {
                        // this connector cannot be used, so block it
                        connector.Used = false;
                    }
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

        /// <summary>
        /// Whether a room can be placed at a specific grid location.
        /// </summary>
        /// <returns></returns>
        private bool CanPlaceRoomAtCoordinate(Room room, int x, int y)
        {
            int stepX = room.Width / StageManager.StepSize;            
            int gridRoomHeight = room.Length;
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

        private bool IsInBounds(int x, int y)
        {
            if (x < 0 || y < 0 ||
                x >= this.dimensions ||
                y >= this.dimensions)
            {
                return false;
            }

            return true;
        }

        public Vector3 GetWorldCoordsFromGridCoords(int x, int y, int objWidth, int objHeight)
        {
            // Center at 0, 0
            int center = this.dimensions / 2;            
            int xWorld = (x - center) * StageManager.StepSize;
            int yWorld = (y - center) * StageManager.StepSize;
            int depth = 0;
            // Center the object from bottom-left to its center
            xWorld = xWorld + objWidth / 2;
            yWorld = yWorld + objHeight / 2;
            return new Vector3(xWorld, depth, yWorld);
        }

        /// <summary>
        /// Returns true if the room can be added to the grid based on the open connectors.
        /// </summary>
        public bool CanAddRoom(Room room)
        {
            foreach (var connector in this.openConnections)
            {
                if (FindRoomPlacement(room, connector) != null)
                {
                    return true;
                }
            }

            return false;
        }

        public List<Room> GetPossibleRooms(List<Room> allRooms, LevelGenRequirements reqs)
        {
            List<Room> possibleRooms = new();
            foreach (var room in allRooms)
            {
                if (this.CanAddRoom(room) && room.HasMatchingExhibit(reqs.SectionData))
                {
                    possibleRooms.Add(room);
                }   
            }

            return possibleRooms;
        }

        public RoomData AddFirstRoom(Room room)
        {
            int center = this.dimensions / 2;
            RoomData data = this.GetRoomData(room, center, center);
            this.PlaceRoomAtCoordinate(data, center, center);
            return data;
        }

        public RoomData AddRoomToGrid(Room room)
        {
            Queue<OpenConnectorCell> openConnectors = new Queue<OpenConnectorCell>();
            foreach (OpenConnectorCell connector in this.openConnections)
            {
                openConnectors.Enqueue(connector);
            }

            while (openConnectors.Count > 0)
            {
                OpenConnectorCell currentOpenConnectorCell = openConnectors.Dequeue();

                RoomPlacement roomPlacement = this.FindRoomPlacement(room, currentOpenConnectorCell);
                if (roomPlacement == null)
                {
                    continue;
                }

                var x = roomPlacement.CoordX;
                var y = roomPlacement.CoordY;

                this.PlaceRoomAtCoordinate(roomPlacement.RoomData, x, y);
                this.openConnections.Remove(currentOpenConnectorCell);
                
                roomPlacement.MatchedConnector.Used = true;
                roomPlacement.OtherConnector.Used = true;
                return roomPlacement.RoomData;
            }

            Debug.LogError("No possible room prefabs could be placed to satisfy any open connector or condition. Level generation may be stuck or incomplete.");
            return null;
        }

        /// <summary>
        /// Finds a placement for the room based on the open connector cell. Returns null if no placement is found.
        /// </summary>
        /// <param name="openConnectorCell">A connector already available to be used in the grid.</param>
        /// <returns></returns>
        private RoomPlacement FindRoomPlacement(Room room, OpenConnectorCell openConnectorCell)
        {
            // Find all connectors on the room that match the open connector
            var matchingConnectors = room.Connectors.Where(connector => connector.Data.IsMatchingConnector(openConnectorCell.OpenConnector)).ToList();
            if (matchingConnectors.Count == 0)
            {
                // No matching connector found
                return null;
            }

            // Try each matching connector to see if the room can fit
            foreach (var connector in matchingConnectors)
            {
                Vector2 roomOffset = connector.GetRelativeGridCoords();
                int roomCoordX = openConnectorCell.X - (int)roomOffset.x;
                int roomCoordY = openConnectorCell.Y - (int)roomOffset.y;
                if (this.CanPlaceRoomAtCoordinate(room, roomCoordX, roomCoordY))
                {
                    RoomData data = this.GetRoomData(room, roomCoordX, roomCoordY);
                    var connectorData = connector.ToRoomConnectorData();
                    return new RoomPlacement(data, connectorData, openConnectorCell.OpenConnector, roomCoordX, roomCoordY);
                }
            }

            // No valid placement found
            return null;
        }

        private RoomData GetRoomData(Room room, int x, int y)
        {
            RoomData data = room.ToRoomData();
            data.RoomReference = room;
            int height = room.Length;

            data.WorldCoords = this.GetWorldCoordsFromGridCoords(x, y, room.Width, height);
            data.GridCoords = new Vector2(x, y);
            return data;
        }

        private class RoomPlacement
        {
            // public Room Room { get; private set; }

            public RoomData RoomData { get; private set; }

            /// <summary>
            /// The connector in this room that was matched to an open connector.
            /// </summary>
            public RoomConnectorData MatchedConnector { get; private set; }

            /// <summary>
            /// An open connector existing in the grid that led to this room.
            /// </summary>
            public RoomConnectorData OtherConnector { get; private set; }
            public int CoordX { get; private set; }
            public int CoordY { get; private set; }

            /// <summary>
            /// A room with a connector that can lead to this room. Used to find
            /// viable rooms and the connector that can lead to them. CoordX and CoordY
            /// are the grid coords for where the room will be placed in this scenario.
            /// </summary>
            public RoomPlacement(RoomData roomData, RoomConnectorData matchedConnector, RoomConnectorData otherConnector, int coordX, int coordY)
            {
                this.RoomData = roomData;
                this.MatchedConnector = matchedConnector;
                this.OtherConnector = otherConnector;
                this.CoordX = coordX;
                this.CoordY = coordY;
            }
        }
    }
}

