using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ArticleText : MonoBehaviour 
{
    public int WordsPerLine = 8;
    public int LinesPerSegment = 5;

    public TextMesh textMesh;

    int currentSegment = 0;
    public List<string> textSegments = new List<string>();

    public string FullText;

	// Use this for initialization
	void Awake() 
    {
        // Must be activated dynamically
        this.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () 
    {	
	}

    public void SetArticleText(string allText, List<string> keyWords = null)
    {        
        if (string.IsNullOrEmpty(allText))
        {
            this.SetEmptyText();
        }

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
                    string highlighted = HighlightKeywords(segment.ToString(), keyWords);
                    textSegments.Add(highlighted);
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
            string highlighted = HighlightKeywords(segment.ToString(), keyWords);
            textSegments.Add(highlighted);
        }

        if (this.textSegments.Count == 0)
        {
            this.SetEmptyText();
        }
        
        textMesh.text = this.textSegments[0];
    }

    private string HighlightKeywords(string text, List<string> keyWords)
    {
        if (keyWords == null || string.IsNullOrEmpty(text))
        {
            return text;
        }

        // Special case keywords
        keyWords.RemoveAll(a => a == "color" || a == "blue");

        foreach (string keyWord in keyWords)
        {
            text = text.Replace(keyWord, "<color=blue>" + keyWord + "</color>");
        }

        return text;
    }

    public void SetEmptyText()
    {
        this.textSegments.Add("There is no text here");
        this.textSegments.Add("Nope, nothing");
    }

    public void MoveToNextSegment()
    {
        if (++currentSegment >= this.textSegments.Count)
        {
            currentSegment = 0;
        }

        textMesh.text = this.textSegments[currentSegment];
    }
}
