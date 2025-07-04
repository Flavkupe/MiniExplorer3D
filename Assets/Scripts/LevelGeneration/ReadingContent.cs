using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;


public class ReadingContent : MonoBehaviour, ICanLookAtAndInteract
{
    /// <summary>
    /// Displayed text, floating.
    /// </summary>
    public ArticleText TextFloat;

    /// <summary>
    /// Displayed text, on the prefab.
    /// </summary>
    public ArticleText TextDisplay;

    // Object which holds this one, and can be disabled if we want to not show
    //  anything for this component.
    public GameObject HoldingEntity;

    public LinkBoard LinkBoard;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (this.TextFloat != null)
        {
            this.TextFloat.gameObject.SetActive(false);
        }
	}    

    public void AddText(LocationTextData textData)
    {
        List<string> keyWords = textData.LinkedLocationData.Select(a => a.DisplayName).ToList();
        if (this.TextFloat != null)
        {
            this.TextFloat.SetArticleText(textData.Text, keyWords);
        }

        if (this.TextDisplay != null)
        {
            this.TextDisplay.SetArticleText(textData.Text, keyWords);
        }

        if (this.LinkBoard != null)
        {
            this.LinkBoard.SetLinks(textData.LinkedLocationData);
        }
    }

    public void HideWholeEntity()
    {
        if (this.HoldingEntity != null)
        {
            this.HoldingEntity.gameObject.SetActive(false);
        }
    }

    public void SetEmptyText()
    {
        if (this.TextFloat != null)
        {
            this.TextFloat.SetEmptyText();
        }
    }

    //void OnTriggerEnter(Collider other)
    //{
    //    if (other.transform.gameObject.IsPlayer())
    //    {
    //        if (this.TextFloat != null)
    //        {                
    //            this.TextFloat.gameObject.SetActive(true);
    //        }
    //    }
    //}

    //void OnTriggerStay(Collider other)
    //{
    //    if (other.transform.gameObject.IsPlayer())
    //    {
    //        if (this.TextFloat != null && Input.GetKeyUp(KeyCode.Space))
    //        {
    //            this.TextFloat.MoveToNextSegment();
    //        }
    //    }
    //}

    //void OnTriggerExit(Collider other)
    //{
    //    if (other.transform.gameObject.IsPlayer())
    //    {
    //        if (this.TextFloat != null)
    //        {
    //            this.TextFloat.gameObject.SetActive(false);
    //        }
    //    }
    //}

    public void LookAt(GameObject source)
    {
        if (this.TextFloat != null)
        {                       
            this.TextFloat.gameObject.SetActive(true);
        }        
    }

    public bool InteractWith(GameObject source, KeyCode key)
    {
        if (this.TextFloat != null)
        {
            this.TextFloat.MoveToNextSegment();
            return true;
        }

        return false;
    }
}
