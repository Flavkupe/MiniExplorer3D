using UnityEngine;
using System.Collections;
using System;

public class SceneGrid : MonoBehaviour
{
    public float width = 32.0f;
    public float height = 32.0f;

    public Color color;

    void Start()
    {
    }

    void Update()
    {
    }

    void OnDrawGizmos()
    {
        Vector3 pos = Camera.current.transform.position;

        if (width == 0 || height == 0)
        {
            return;
        }

        Gizmos.color = this.color;        

        for (float z = pos.z - 800.0f; z < pos.z + 800.0f; z += height)
        {            
            Gizmos.DrawLine(new Vector3(-1000000.0f, 0.0f, Mathf.Floor(z / height) * height),
                            new Vector3(1000000.0f, 0.0f, Mathf.Floor(z / height) * height));
        }

        for (float x = pos.x - 1200.0f; x < pos.x + 1200.0f; x += width)
        {
            Gizmos.DrawLine(new Vector3(Mathf.Floor(x / width) * width, 0.0f, -1000000.0f),
                            new Vector3(Mathf.Floor(x / width) * width, 0.0f, 1000000.0f));
        }

        Gizmos.color = new Color(this.color.r, this.color.g, this.color.b, Math.Min(1.0f, this.color.a * 2.0f));

        Gizmos.DrawLine(new Vector3(0, 0, -1000000.0f), new Vector3(0, 0, 1000000.0f));
        Gizmos.DrawLine(new Vector3(-1000000.0f, 0.0f, 0.0f), new Vector3(1000000.0f, 0.0f, 0.0f));
    }
}