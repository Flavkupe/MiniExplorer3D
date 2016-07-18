using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ArticleText : ThreeDTextBase 
{
    public int WordsPerLine = 8;
    public int LinesPerSegment = 5;    

    int currentSegment = 0;
    public List<string> textSegments = new List<string>();

    public string FullText;

	// Use this for initialization
	void Awake() 
    {
        this.InitializeText();

        // Must be activated dynamically
        this.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () 
    {	
	}

    public void SetArticleText(string allText)
    {
        this.FullText = allText;
        int wordCounter = 0;
        int lineCounter = 0;
        StringBuilder segment = new StringBuilder();
        foreach (string word in allText.Split(' '))
        {
            segment.Append(word);
            if (++wordCounter >= WordsPerLine)
            {
                if (++lineCounter >= LinesPerSegment)
                {
                    segment.Append(" [...]");
                    textSegments.Add(segment.ToString());
                    segment = new StringBuilder();
                    lineCounter = 0;
                }
                else
                {
                    segment.AppendLine();
                }

                wordCounter = 0;
            }
            else 
            {
                segment.Append(' ');
            }
        }

        if (segment.Length > 0)
        {
            this.textSegments.Add(segment.ToString());
        }

        if (this.textSegments.Count == 0)
        {
            this.textSegments.Add("There is no text here");
            this.textSegments.Add("Nope, nothing");
        }
        
        this.UpdateTextMeshes(this.textSegments[0]);
    }

    public void MoveToNextSegment()
    {
        if (++currentSegment >= this.textSegments.Count)
        {
            currentSegment = 0;
        }

        this.UpdateTextMeshes(this.textSegments[currentSegment]);               
    }
}
