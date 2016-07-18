using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMesh))]
public class NameTag : ThreeDTextBase
{
	private IHasName owner;

    void Awake()
    {
        this.InitializeText();
    }

	// Use this for initialization
	void Start () 
    {
		this.owner = this.transform.parent.GetComponent(typeof(IHasName)) as IHasName;		
		this.RefreshName(); 
	}
	
	// Update is called once per frame
	void Update () 
    {	
	}

	public void RefreshName() 
	{
		if (this.owner != null)  
		{
			this.UpdateTextMeshes(this.owner.GetName());			
		}
	}
}
