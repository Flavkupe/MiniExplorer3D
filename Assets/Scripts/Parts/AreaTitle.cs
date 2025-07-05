using UnityEngine;
using System.Collections;

public class AreaTitle : MonoBehaviour 
{
    private TMPro.TextMeshPro childTextMesh;

	void Awake() {
        this.childTextMesh = this.GetComponentInChildren<TMPro.TextMeshPro>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetTitle(string text)
    {
        if (this.childTextMesh != null)
        {
            this.childTextMesh.text = text;
        }
    }
}
