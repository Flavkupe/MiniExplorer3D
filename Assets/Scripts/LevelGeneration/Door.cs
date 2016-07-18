
using System;
using UnityEngine;

public class Door : MonoBehaviour, IHasName, IHasLocation
{
	private NameTag nametag;

    public DoorData Data = new DoorData();

	void Start() 
	{
		this.nametag = this.GetComponentInChildren<NameTag>();
	}

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
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
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            bool transitioned = StageManager.AttemptTransition(this.GetLocation());
        }
    }

	public void SetName(string name) 
	{ 
		this.Data.DisplayName = name; 
		if (this.nametag != null) 
		{
			this.nametag.RefreshName();
		}
	}

	public string GetName() { return this.Data.DisplayName; }

    public void SetLocation(Location location) { this.Data.Location = location; }
    public Location GetLocation() { return this.Data.Location; }

    public DoorData ToDoorData()
    {
        DoorData data = this.Data.Clone();
        data.PrefabID = this.name;
        return data; 
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