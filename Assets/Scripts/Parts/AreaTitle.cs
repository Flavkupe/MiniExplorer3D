using UnityEngine;
using System.Collections;

public class AreaTitle : MonoBehaviour 
{
    private TMPro.TextMeshPro childTextMesh;

    public int MaxRows = 3;
    public int MaxCharsPerRow = 13;

	// Use this for initialization
	void Awake() {
        this.childTextMesh = this.GetComponentInChildren<TMPro.TextMeshPro>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetTitle(string text)
    {
        int maxChars =  MaxRows * MaxCharsPerRow;
        if (this.childTextMesh != null)
        {
            for (int i = MaxCharsPerRow; i < maxChars; i += MaxCharsPerRow)
            {
                if (i < text.Length)
                {                                        
                    text = text.Insert(i, "\n");                                        
                }                
            }

            if (text.Length > maxChars)
            {
                text = text.Substring(0, maxChars);
            }

            this.childTextMesh.text = text;
        }
    }
}
