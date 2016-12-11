using UnityEngine;
using System.Collections;


public class TiledQuad : MonoBehaviour 
{
    MeshFilter meshFilter;    

    public float ScaleFactor = 1.0f;

	// Use this for initialization
	void Start () 
    {
        meshFilter = this.GetComponent<MeshFilter>();        

        if (meshFilter != null)
        {
            if (ScaleFactor <= 0.0f)
            {
                ScaleFactor = 1.0f;
            }

            float width = this.transform.localScale.x / ScaleFactor;
            float height = this.transform.localScale.y / ScaleFactor;

            meshFilter.mesh.uv = new Vector2[] 
            {
                new Vector2(0, 0),
                new Vector2(width, height),
                new Vector2(width, 0),
                new Vector2(0, height)
            };
        }
	}
	
	// Update is called once per frame
	void Update () 
    {        
	}
}
