using UnityEngine;
using System.Collections;

public class ReadingContent : MonoBehaviour 
{    
    public ArticleText TextFloat;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void AddText(string text)
    {
        if (this.TextFloat != null)
        {
            this.TextFloat.SetArticleText(text);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject.IsPlayer())
        {
            if (this.TextFloat != null)
            {                
                this.TextFloat.gameObject.SetActive(true);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.transform.gameObject.IsPlayer())
        {
            if (this.TextFloat != null && Input.GetKeyUp(KeyCode.Space))
            {
                this.TextFloat.MoveToNextSegment();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.transform.gameObject.IsPlayer())
        {
            if (this.TextFloat != null)
            {
                this.TextFloat.gameObject.SetActive(false);
            }
        }
    }
}
