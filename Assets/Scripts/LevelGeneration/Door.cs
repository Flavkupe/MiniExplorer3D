
using System;
using UnityEngine;

public class Door : MonoBehaviour, IHasName, IHasLocation
{
	private NameTag nametag;

    [Tooltip("TMP object that will optionally show name of the door, if provided.")]
    public TMPro.TextMeshPro label;

    private BoxCollider boxCollider;

    public bool RemoveOnUnused = true;

    public DoorData Data = new DoorData();

	void Start() 
	{
		this.nametag = this.GetComponentInChildren<NameTag>();
        this.label = this.GetComponentInChildren<TMPro.TextMeshPro>();
        if (this.label != null)
        {
            this.label.text = this.Data.DisplayName;
        }
    }

    void Awake()
    {
        this.boxCollider = this.GetComponent<BoxCollider>();
    }
	
	void FixedUpdate() 
    {
        if (nametag != null && nametag.gameObject.activeSelf)
        {
            nametag.gameObject.SetActive(false);
        }
	}

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            this.OnPlayerTouching();    
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            this.OnPlayerTouching();
        }
    }

    private void OnPlayerTouching()
    {
        if (!nametag.gameObject.activeSelf)
        {
            nametag.gameObject.SetActive(true);
            nametag.RefreshName();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            StageManager.AttemptTransition(this.GetLocation());
        }
    }

	public void SetName(string name) 
	{ 
		this.Data.DisplayName = name;
        if (this.label != null)
        {
            this.label.text = name;
        }
    }

	public string GetName() { return this.Data.DisplayName; }

    public void SetLocation(Location location) { this.Data.Location = location; }

    public void SetLocationData(LinkedLocationData data)
    {
        this.Data.Location = new MainLocation(data.Path, data.DisplayName);
        this.SetName(data.DisplayName);
    }

    public Location GetLocation() { return this.Data.Location; }

    public DoorData ToDoorData()
    {
        DoorData data = this.Data.Clone();
        data.PrefabID = this.name;
        return data; 
    }


    public void TeleportObjectToFront(GameObject gameObject)
    {
        gameObject.transform.position = this.transform.position + (transform.localRotation * this.boxCollider.center);
    }
}

[Serializable]
public class DoorData : IMatchesPrefab
{
    public string PrefabID { get; set; }

    /// <summary>
    /// Name of specific room in area (such as folder name)
    /// </summary>
    public string DisplayName;

    /// <summary>
    /// Where this leads to, in terms of Area (such as path)
    /// </summary>
    public Location Location;

    public DoorData Clone()
    {
        return new DoorData() { DisplayName = DisplayName, Location = Location, PrefabID = this.PrefabID };
    }
}