using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ThreeDTextBase : MonoBehaviour
{
    public TextMesh textMeshComponent;
    public GameObject shadowtext;
    public bool UseShadow = true;

    private TextMesh shadowtextMesh;    
    
    protected void InitializeText()
    {
        if (this.UseShadow && this.shadowtext != null)
        {
            this.shadowtextMesh = this.shadowtext.GetComponent<TextMesh>();
            this.shadowtextMesh.gameObject.SetActive(true);
        }
    }

    protected void UpdateTextMeshes(string text)
    {
        this.textMeshComponent.text = text;
        if (this.UseShadow && this.shadowtextMesh != null)
        {
            this.shadowtextMesh.text = text;
        }
    }
}

