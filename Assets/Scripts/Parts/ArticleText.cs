using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class ArticleText : MonoBehaviour, ICanSupportTitle
{
    /// <summary>
    /// Whether to split the text by lines. Should
    /// be disabled when using TMPro, which handles this for us.
    /// </summary>
    public bool SplitLines = true;

    public int WordsPerLine = 8;
    public int LinesPerSegment = 5;

    public TextMesh textMesh;

    public TMPro.TextMeshPro textMeshPro;

    public TMPro.TextMeshPro titleTextMesh;

    int currentSegment = 0;
    public List<string> textSegments = new List<string>();

    public string FullText;

    public bool SupportsTitle => titleTextMesh != null;
	
	// Update is called once per frame
	void Update () 
    {	
	}

    public void SetTitle(string title)
    {
        if (this.titleTextMesh != null)
        {
            this.titleTextMesh.SetText(title);
        }
    }

    public void SetArticleText(string allText, List<string> keyWords = null)
    {        
        if (string.IsNullOrEmpty(allText))
        {
            this.SetEmptyText();
        }

        string highlighted = HighlightKeywords(allText, keyWords);
        this.FullText = highlighted;

        if (!SplitLines)
        {
            textSegments.Add(highlighted);
            SetText(this.textSegments[0]);
            return;
        }

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
            textSegments.Add(segment.ToString());
        }

        if (this.textSegments.Count == 0)
        {
            this.SetEmptyText();
        }

        SetText(this.textSegments[0]);
    }

    private void SetText(string text)
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = text;
        }

        if (textMesh != null)
        {
            textMesh.text = text;
        }
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

        SetText(this.textSegments[currentSegment]);
    }
}
