using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class SpawnPoint : MonoBehaviour 
{
    public SpawnPointData Data = new SpawnPointData();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public SpawnPointData ToSpawnPointData()
    {
        SpawnPointData data = this.Data.Clone();
        data.PrefabID = this.name;
        return data;
    }
}

[Serializable]
public class SpawnPointData : IMatchesPrefab
{
    public SpawnPointType Type = SpawnPointType.NPC;

    public EnemyType EnemyTypes;

    public string Entity { get; set; }

    public string PrefabID { get; set; }

    public SpawnPointData Clone()
    {
        SpawnPointData clone = new SpawnPointData();
        clone.PrefabID = this.PrefabID;
        clone.Type = this.Type;
        clone.Entity = this.Entity;
        clone.EnemyTypes = this.EnemyTypes;

        return clone;
    }
}

public enum SpawnPointType
{
    NPC = 1,
    EnemyOnly = 2,
}
