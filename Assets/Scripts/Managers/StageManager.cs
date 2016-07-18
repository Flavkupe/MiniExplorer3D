using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.LevelGeneration;
using UnityEngine.SceneManagement;

public static class StageManager 
{
    public const int StepSize = 8;
    public const int RoomGridDimensions = 40;
    public const int MaxAreaEntities = 2;
    public const int MaxRoomDoors = 300;

    private static Dictionary<string, RoomGrid> knownAreaMap = new Dictionary<string, RoomGrid>();
    private static ILevelGenerator levelGenerator = new FileLevelGenerator();

    public static Dictionary<string, RoomGrid> KnownAreaMap
    {
        get { return StageManager.knownAreaMap; }
    }
    
    public static ILevelGenerator LevelGenerator
    {
        get { return StageManager.levelGenerator; }        
    }

    public static RoomGrid GetAreaRoomGridOrNull(Location location)
    {
        return knownAreaMap.GetValueOrDefault(location.LocationKey);
    }

    public static GameObject Player { get; set; }

    public static SceneLoader SceneLoader = null;
    
    public static Location CurrentLocation = null;
    public static Location PreviousLocation = null;
    public static Area CurrentArea = null;

    public static bool AttemptTransition(Location location) 
    {
        if (levelGenerator.CanLoadLocation(location))
        {
            PreviousLocation = CurrentLocation;
            CurrentLocation = location;
            SceneManager.LoadScene("Area");
            return true;
        }
        
        return false;       
    }

    public static Vector2 GetGridCoordsFromWorldCoords(Vector3 worldLoc)
    {        
        int gridCenter = StageManager.RoomGridDimensions / 2;

        int xGrid = (int)worldLoc.x / StageManager.StepSize;
        int yGrid = (int)worldLoc.y / StageManager.StepSize;

        // Center the object on grid
        xGrid = xGrid + gridCenter;
        yGrid = yGrid + gridCenter;
        return new Vector2(xGrid, yGrid);
    }

    public static void SetLevelGenMode(LevelGenerationMode levelGenerationMode)
    {
        if (levelGenerationMode == LevelGenerationMode.File &&
            !(levelGenerator is FileLevelGenerator)) 
        {
            levelGenerator = new FileLevelGenerator();
        }
        else if (levelGenerationMode == LevelGenerationMode.Web &&
            !(levelGenerator is WebLevelGenerator))
        {
            levelGenerator = new WebLevelGenerator();
        }
    }
}

