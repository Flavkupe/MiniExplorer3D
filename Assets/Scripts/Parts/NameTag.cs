using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMesh))]
public class NameTag : MonoBehaviour
{
	private IHasName owner;

    private TextMesh textMesh;

    void Awake()
    {
        this.textMesh = GetComponent<TextMesh>();
    }

	// Use this for initialization
	void Start () 
    {
		this.owner = this.transform.parent.GetComponent(typeof(IHasName)) as IHasName;
	}
	
	// Update is called once per frame
	void Update () 
    {        
	}

    public void RefreshName()
    {
        this.textMesh.text = this.owner.Name;
    }
}
