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

    public abstract RoomGrid GenerateRoomGrid(Location targetLocation);

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
        this.sectionData.AddRange(locationData.Sections);

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
