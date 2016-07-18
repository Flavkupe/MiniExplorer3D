using UnityEngine;
using System.Collections;

public class ClosingWall : MonoBehaviour {

    public Vector3 NormalScale;
    public Vector3 ModifiedScale;

    public Vector3 NormalPosition;
    public Vector3 ModifiedPosition;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SwitchStance(bool modified)
    {
        if (modified)
        {
            this.transform.localScale = ModifiedScale;
            this.transform.localPosition = ModifiedPosition;
        }
        else     
        {
            this.transform.localScale = NormalScale;
            this.transform.localPosition = NormalPosition;
        }
    }
}
