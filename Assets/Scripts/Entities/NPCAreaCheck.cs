using UnityEngine;
using System.Collections;
using System;

public class NPCAreaCheck : MonoBehaviour 
{
    public event EventHandler<ObjectDetectedEventArgs> PlayerDetected;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.transform.tag == "Player")
        {
            if (PlayerDetected != null)
            {
                PlayerDetected(this, new ObjectDetectedEventArgs(this, other));
            }
        }
    }
}

public class ObjectDetectedEventArgs : EventArgs
{
    public Collider2D OtherCollider;
    public NPCAreaCheck AreaCheck;
    
    public ObjectDetectedEventArgs()
    {
    }

    public ObjectDetectedEventArgs(NPCAreaCheck me, Collider2D other)
    {
        this.AreaCheck = me;
        this.OtherCollider = other;
    }

    public ObjectDetectedEventArgs(Collider2D other)
    {
        this.OtherCollider = other;
    }
}