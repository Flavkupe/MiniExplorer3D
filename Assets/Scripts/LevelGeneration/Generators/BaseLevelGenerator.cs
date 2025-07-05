using Assets.Scripts.LevelGeneration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public abstract class BaseLevelGenerator : ILevelGenerator
{
    protected virtual Room GetFirstRoom(Location targetLocation)
    {
        Room startingRoom = StageManager.SceneLoader.StartingRoomPrefabs.GetRandom();
        startingRoom.PopulateParts();
        return startingRoom;
    }

    public virtual RoomGrid GenerateRoomGrid(Location targetLocation)
    {                
        RoomGrid grid = new RoomGrid(StageManager.RoomGridDimensions);
        AreaTheme theme = GetAreaTheme(targetLocation);
        grid.AreaTheme = theme;
        Queue<Location> locations = new Queue<Location>();
        Queue<string> entities = new Queue<string>();
        Location backLocation = this.GetBackLocation(targetLocation);
        if (backLocation != null)
        {
            locations.Enqueue(backLocation);
        }

        int entityCount = 0;
        foreach (string entity in GetLevelEntities(targetLocation))
        {
            if (entityCount > StageManager.MaxAreaEntities)
            {
                break;
            }

            entities.Enqueue(entity);
        }

        this.ProcessLocation(targetLocation);        
        locations.EnqueueRange(targetLocation.LocationData.SubLocations);        

        List<Room> possibleRooms = null;
        if (StageManager.SceneLoader.RoomPrefabs.Length > 0)
        {
            possibleRooms = StageManager.SceneLoader.RoomPrefabs.ToList();
        }
        else
        {
            possibleRooms = ResourceManager.GetAllRoomPrefabs(theme);
        }
        foreach (Room room in possibleRooms)
        {
            room.PopulateParts();
        }

        // Pick a starting room
        Room startingRoom = this.GetFirstRoom(targetLocation);

        Location currentLocation = null;
        List<RoomData> rooms = new List<RoomData>();
        RoomData currentRoomData = grid.AddFirstRoom(startingRoom);

        do
        {
            for (int i = 0; i < currentRoomData.Doors.Count; ++i)
            {
                if (locations.Count != 0)
                {
                    currentLocation = locations.Dequeue();
                    Location loc = currentLocation.Clone();                    
                    currentRoomData.Requirements.Locations.Enqueue(loc);
                }
            }

            for (int i = 0; i < currentRoomData.SpawnPoints.Count; ++i)
            {
                if (entities.Count != 0)
                {
                    string currentEntity = entities.Dequeue();
                    currentRoomData.SpawnPoints[i].Entity = currentEntity;
                }
            }

            // TEMP?: Probably best to only try to fit in the locations and not entities
            //if (locations.Count > 0 || entities.Count > 0)
            if (locations.Count > 0)
            {
                currentRoomData = grid.AddRoomFromList(possibleRooms, currentRoomData.Requirements);
                if (currentRoomData != null)
                {
                    rooms.Add(currentRoomData);
                }
            }

        } while ((locations.Count > 0 || entities.Count > 0) && currentRoomData != null);

        return grid;
    }

    protected abstract void ProcessLocation(Location location);

    protected abstract AreaTheme GetAreaTheme(Location location);

    protected abstract Location GetBackLocation(Location currentLcation);

    public abstract List<string> GetLevelEntities(Location location);

    public abstract bool CanLoadLocation(Location location);

    public virtual bool NeedsAreaGenPreparation { get { return false; } }

    public virtual IEnumerator PrepareAreaGeneration(Location location, MonoBehaviour caller)
    {
        this.CallOnAreaGenReady(new AreaGenerationReadyEventArgs() { AreaLocation = location });
        yield return null;
    }

    protected void CallOnAreaGenReady(AreaGenerationReadyEventArgs e)
    {
        if (OnAreaGenReady != null)
        {
            OnAreaGenReady(this, e);
        }
    }


    protected void CallOnAreaPostProcessingDone(AreaGenerationReadyEventArgs e)
    {
        if (OnAreaPostProcessingDone != null)
        {
            OnAreaPostProcessingDone(this, e);
        }
    }

    public virtual IEnumerator AreaPostProcessing(Location location, MonoBehaviour caller)
    {
        yield return null;
    }

    public event EventHandler<AreaGenerationReadyEventArgs> OnAreaGenReady;
    public event EventHandler<AreaGenerationReadyEventArgs> OnAreaPostProcessingDone;
}

public enum LevelGenerationMode 
{
    File,
    Wikipedia,
    Debug
}

public class AreaGenerationReadyEventArgs : EventArgs
{
    public Location AreaLocation { get; set; }
}

public class LevelGenRequirements
{
    private Queue<Location> locations = new Queue<Location>();
    public Queue<Location> Locations => locations;

    private List<SectionData> sectionData = new();
    public List<SectionData> SectionData => sectionData;

    private Queue<ExhibitData> exhibitData = new();
    public Queue<ExhibitData> ExhibitData => exhibitData;

    public TableOfContents TableOfContents { get; set; }

    public LevelGenRequirements() { }

    public LevelGenRequirements(Location location)
    {
        var locationData = location?.LocationData;
        if (locationData == null)
        {
            Debug.LogWarning($"LocationData is null for location: {location?.Name}");
            return;
        }

        // Traverse SectionData to fill requirements
        if (locationData.Sections != null)
        {
            this.sectionData.AddRange(locationData.Sections);
        }

        // TableOfContents if present
        this.TableOfContents = locationData?.TableOfContents;
        this.Locations.Enqueue(location);
    }

    /// <summary>
    /// The base reqs are just that all locations are linked, nothing else.
    /// </summary>
    public virtual bool AllRequirementsMet => this.Locations.Count == 0;

    protected virtual LevelGenRequirements GetInstance() 
    {
        return new LevelGenRequirements();        
    }

    public LevelGenRequirements Clone(bool deepCopy = true)
    {
        LevelGenRequirements copy = this.GetInstance();
        this.locations.ToList().ForEach(item => copy.locations.Enqueue(item.Clone(deepCopy)));
        this.sectionData.ToList().ForEach(item => copy.sectionData.Add(item));
        copy.TableOfContents = this.TableOfContents;
        return copy;
    }
}
