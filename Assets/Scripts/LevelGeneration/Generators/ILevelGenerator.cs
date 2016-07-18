
using Assets.Scripts.LevelGeneration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILevelGenerator
{
    RoomGrid GenerateRoomGrid(Location targetLocation);

    List<string> GetLevelEntities(Location location);
    List<LevelImage> GetLevelImages(Location location);
    
    bool CanLoadLocation(Location location);

    IEnumerator PrepareAreaGeneration(Location location, MonoBehaviour caller);
    event EventHandler<AreaGenerationReadyEventArgs> OnAreaGenReady;

    bool NeedsAreaGenPreparation { get; }
}


