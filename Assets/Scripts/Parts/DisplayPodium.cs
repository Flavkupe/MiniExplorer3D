using UnityEngine;
using System.Collections;

public class DisplayPodium : MonoBehaviour 
{
    public RoomImageFrame ImageFrame;

    public InfoBoxDisplay InfoBoxDisplay;

    public void SetImage(LevelImage image)
    {
        if (this.ImageFrame != null)
        {
            this.ImageFrame.SetLevelImage(image);
            this.ImageFrame.gameObject.SetActive(true);
            this.IsUsed = true;
        }
    }

    public void SetText(InfoBoxData data)
    {
        if (this.InfoBoxDisplay)
        {
            this.InfoBoxDisplay.SetInfoBoxData(data);
            this.IsUsed = true;            
        }
    }

    public bool CanSetImage { get { return this.ImageFrame != null; } }
    public bool CanSetText { get { return this.InfoBoxDisplay != null; } }

	// Use this for initialization
	void Start () {
        this.InfoBoxDisplay = this.GetComponentInChildren<InfoBoxDisplay>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public bool IsUsed { get; set; }
}
