using UnityEngine;
using System.Collections;
using System;

public class ClickableText : MonoBehaviour , ICanLookAtAndInteract, IHasName
{
    TextMesh textMesh;

    Action<object> clickCallback;
    object actionParam;
    
    private string normalText;
    private string fullText;

    public bool ExpandOnLook = false; 

    bool focused = false;

    public bool HighlightsOnFocus = true;

    public NameTag expandedName;

	// Use this for initialization
	void Awake () {
        this.textMesh = GetComponentInChildren<TextMesh>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (HighlightsOnFocus)
        {
            if (!focused)
            {
                //this.textMesh.text = normalText;
                this.textMesh.color = Color.white;
            }
            else
            {
                //this.textMesh.text = highlightedText;
                this.textMesh.color = Color.blue;
            }
        }

        if (this.expandedName != null)
        {
            this.expandedName.gameObject.SetActive(false);
        }

        focused = false;
	}

    public bool InteractWith(GameObject source, KeyCode key)
    {
        if (this.clickCallback != null && this.focused)
        {
            this.clickCallback(this.actionParam);
            return true;
        }

        return false;
    }

    public void LookAt(GameObject source)
    {
        focused = true;

        if (this.fullText != null && this.ExpandOnLook && this.expandedName != null)
        {
            this.expandedName.RefreshName();
            this.expandedName.gameObject.SetActive(true);
        }
    }

    public void SetText(string text, Action<object> action = null, object actionParam = null)
    {
        this.clickCallback = action;
        this.actionParam = actionParam;
        this.normalText = text;
        this.textMesh.text = text;
    }

    public void SetFullText(string fullText)
    {
        this.fullText = fullText;
    }

    public string GetName()
    {
 	    return this.fullText ?? this.normalText;
    }
}
