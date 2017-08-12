using UnityEngine;
using System.Collections;

public class LoadingView : MonoBehaviour {

    public GameObject LoadingUI;

	// Use this for initialization
	void Awake () {
        StageManager.LoadingViewer = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ToggleCamera(bool enable) 
    {
        this.LoadingUI.SetActive(enable);
    }
}
