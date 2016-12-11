using UnityEngine;
using System.Collections;

public class FloatyText : MonoBehaviour {

    public float Speed = 1.0f;
    public float FadeDelay = 2.0f;

	// Use this for initialization
	void Start () {
        GameObject.Destroy(this.gameObject, this.FadeDelay);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y + this.Speed, this.transform.localPosition.z);
	}

    public void SetText(string text)
    {
        this.GetComponent<TextMesh>().text = text;
    }
}
