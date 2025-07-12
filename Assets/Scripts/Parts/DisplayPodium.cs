using UnityEngine;
using System.Collections;
using System.Linq;

public class DisplayPodium : MonoBehaviour, ICanSupportTitle
{
    public RoomImageFrame ImageFrame;

    public TMPro.TextMeshPro TextContent;

    public TMPro.TextMeshPro Title;

    public bool SupportsTitle => this.Title != null;

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
            this.IsUsed = true;
        }

        if (Title != null)
        {
            Title.text = title;
            this.IsUsed = true;
        }
    }

    public bool CanSetImage { get { return this.ImageFrame != null; } }
    public bool CanSetText { get { return this.TextContent != null; } }


    public bool IsUsed { get; set; }
}
