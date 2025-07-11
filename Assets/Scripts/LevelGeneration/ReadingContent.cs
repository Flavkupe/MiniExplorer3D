using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;


public class ReadingContent : MonoBehaviour, ICanLookAtAndInteract
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
  
    public void AddText(LocationTextData textData)
    {
        AddText(textData, string.Empty);
    }

    public void AddText(LocationTextData textData, string title)
    {
        var shownTitle = title == string.Empty ? textData.Title : title;
        
        if (this.DisplayPodium != null)
        {
            this.DisplayPodium.SetText(textData, shownTitle);
        }

        List<string> keyWords = textData.LinkedLocationData.Select(a => a.DisplayName).ToList();

        if (this.TextDisplay != null)
        {
            this.TextDisplay.SetArticleText(textData.Text, keyWords);
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
        // TODO
        return false;
    }
}
