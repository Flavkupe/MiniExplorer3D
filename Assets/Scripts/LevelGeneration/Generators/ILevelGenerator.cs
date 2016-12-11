
using Assets.Scripts.LevelGeneration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILevelGenerator
{
    RoomGrid GenerateRoomGrid(Location targetLocation);

    List<string> GetLevelEntities(Location location);
    
    bool CanLoadLocation(Location location);

    IEnumerator PrepareAreaGeneration(Location location, MonoBehaviour caller);
    IEnumerator AreaPostProcessing(Location location, MonoBehaviour caller);
    
    event EventHandler<AreaGenerationReadyEventArgs> OnAreaGenReady;
    event EventHandler<AreaGenerationReadyEventArgs> OnAreaPostProcessingDone;
    

    bool NeedsAreaGenPreparation { get; }
}


