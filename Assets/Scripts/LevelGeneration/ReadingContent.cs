using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;


public class ReadingContent : MonoBehaviour, ICanLookAtAndInteract, ICanSupportTitle
{
    /// <summary>
    /// Displayed text, on the prefab.
    /// </summary>
    public ArticleText TextDisplay;

    // Object which holds this one, and can be disabled if we want to not show
    //  anything for this component.
    public GameObject HoldingEntity;

    /// <summary>
    /// If set, text and title can be displayed here.
    /// </summary>
    public DisplayPodium DisplayPodium;

    public LinkBoard LinkBoard;

    public string Name => this.name;

    private LocationTextData textData;

    public bool SupportsTitle
    {
        get
        {
            return (this.TextDisplay != null && this.TextDisplay.SupportsTitle) ||
                (this.DisplayPodium != null && this.DisplayPodium.SupportsTitle);

        }
    }

    public void AddText(LocationTextData textData)
    {
        AddText(textData, textData.Title);
    }

    public void AddText(LocationTextData textData, string title)
    {
        this.textData = textData;

        var shownTitle = title == string.Empty ? textData.Title : title;
        
        if (this.DisplayPodium != null)
        {
            this.DisplayPodium.SetText(textData, shownTitle);
        }

        List<string> keyWords = textData.LinkedLocationData.Select(a => a.DisplayName).ToList();

        if (this.TextDisplay != null)
        {
            this.TextDisplay.SetArticleText(textData.Text, keyWords);
            this.TextDisplay.SetTitle(title);
        }

        if (this.LinkBoard != null)
        {
            this.LinkBoard.SetLinks(textData.LinkedLocationData);
        }
    }

    public void LookAt(GameObject source)
    {
        // TODO  
    }

    public bool InteractWith(GameObject source, KeyCode key)
    {
        InteractionWindow.Instance.SetText(this.textData, true);
        return true;
    }
}
