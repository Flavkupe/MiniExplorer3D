
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using Assets.Scripts.LevelGeneration;

public class Area : MonoBehaviour, IHasName
{    
    public string DisplayName;
    
    public Room[] Rooms = new Room[] {};

    private RoomGrid roomGrid = null;

    public RoomGrid RoomGrid
    {
        get { return roomGrid; }
        set { roomGrid = value; }
    }

    public AreaTheme Theme
    {
        get 
        { 
            return this.roomGrid != null ? this.roomGrid.AreaTheme : AreaTheme.None; 
        }
        
    }

    void Start() {

    }

    void Awake() 
    {
    }

    void Update() {

    }

    public string Name => this.DisplayName;
    
}

public enum AreaTheme
{
    None = 0,
    Circuit = 1,
    Town = 2,
    Graveyard = 3,
    Forest = 4,
}