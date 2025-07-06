using UnityEngine;
using System.Collections;
using System.Linq;

public class DisplayPodium : MonoBehaviour 
{
    public RoomImageFrame ImageFrame;

    [Tooltip("DEPRECATED: Use TextMeshPro")]
    public InfoBoxDisplay InfoBoxDisplay;

    public TMPro.TextMeshPro TextContent;

    public TMPro.TextMeshPro Title;

    public void SetImage(LevelImage image)
    {
        if (this.ImageFrame != null)
        {
            this.ImageFrame.SetLevelImage(image);
            this.ImageFrame.gameObject.SetActive(true);
            this.IsUsed = true;
        }
    }

    public void SetText(LocationTextData text, string title)
    {
        if (TextContent != null)
        {
            TextContent.text = text.Text;
        }

        if (Title != null)
        {
            Title.text = title;
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
