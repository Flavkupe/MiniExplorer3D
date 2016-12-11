using UnityEngine;
using System.Collections;

public class LoadingView : MonoBehaviour {

    private Camera cam;

	// Use this for initialization
	void Awake () {
        StageManager.LoadingViewer = this;
        this.cam = this.GetComponentInChildren<Camera>();        
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ToggleCamera(bool enable) 
    {
        this.gameObject.SetActive(enable);
    }
}
