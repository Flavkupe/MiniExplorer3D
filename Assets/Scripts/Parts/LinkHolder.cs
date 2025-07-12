using UnityEngine;
using System.Collections;

public class LinkHolder : MonoBehaviour, ICanLookAtAndInteract, IHasName
{
    private LinkedLocationData link;

    public NameTag label;

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        label.gameObject.SetActive(false);
	}

    public void SetLink(LinkedLocationData link)
    {
        this.link = link;
    }

    public void LookAt(GameObject source)
    {
        if (label != null)
        {
            label.gameObject.SetActive(true);
            label.RefreshName();
        }        
    }

    public bool InteractWith(GameObject source, KeyCode key)
    {
        StageManager.AttemptTransition(this.link.Path, this.link.DisplayName);
        return true;
    }

    public string Name
    {
        get
        {
            if (this.link == null)
            {
                return string.Empty;
            }

            return this.link.DisplayName;
        }
    }
}
