using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;


public class ReadingContent : MonoBehaviour, ICanLookAtAndInteract
{    
    public ArticleText TextFloat;

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
        this.TextFloat.gameObject.SetActive(false);
	}    

    public void AddText(LocationTextData textData)
    {
        if (this.TextFloat != null)
        {
            List<string> keyWords = textData.LinkedLocationData.Select(a => a.DisplayName).ToList();
            this.TextFloat.SetArticleText(textData.Text, keyWords);
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
