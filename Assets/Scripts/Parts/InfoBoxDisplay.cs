using UnityEngine;
using System.Collections;

public class InfoBoxDisplay : MonoBehaviour {

    private InfoBoxData data;

    public ClickableText clickableTextModel;
    public ClickableText titleText;

    public int LinesOfContents;
    public int MaxCharsPerLine = 20;
    public float LineHeight = 0.15f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetInfoBoxData(InfoBoxData data)
    {
        this.data = data;

        if (this.clickableTextModel == null || this.clickableTextModel.GetComponentInChildren<TextMesh>() == null)
        {
            return;
        }

        if (this.titleText != null && data.SectionTitle != null)
        {
            string shortenedText = this.ShortenString(data.SectionTitle);
            this.titleText.SetText(shortenedText);
            if (shortenedText != data.SectionTitle)
            {
                this.titleText.SetFullText(data.SectionTitle);
            }
        }

        int currentLine = 0;
        foreach (string[] row in this.data.Rows)
        {
            if (row == null || row.Length == 0)
            {
                continue;
            }
            
            string leftText = row[0] ?? string.Empty;
            string rightText = row[1] ?? string.Empty;

            this.GenerateText(currentLine, leftText, false);
            this.GenerateText(currentLine, rightText, true);

            currentLine++;
            if (currentLine >= LinesOfContents)
            {
                // TEMP
                break;
            }
        }
    }

    private void GenerateText(int currentLine, string text, bool isRightText)
    {
        ClickableText newText = Instantiate(this.clickableTextModel);

        string shortenedText = this.ShortenString(text);
               
        newText.SetText(shortenedText);

        if (shortenedText != text)
        {
            newText.SetFullText(text);
        }

        newText.transform.parent = this.clickableTextModel.transform.parent;
        newText.transform.localRotation = this.clickableTextModel.transform.localRotation;
        newText.transform.localScale = this.clickableTextModel.transform.localScale;
        float offsetDown = (currentLine * this.LineHeight);

        newText.transform.localPosition = this.clickableTextModel.transform.localPosition;
        newText.transform.Translate(Vector3.up * -offsetDown, Space.Self);

        if (isRightText)
        {
            float offsetRight = newText.GetComponent<BoxCollider>().size.x;
            newText.transform.Translate(Vector3.right * offsetRight, Space.Self);
        }
    }

    private string ShortenString(string line, bool cutOffNewlines = true)
    {
        if (cutOffNewlines)
        {
            int indexOfNewline = line.IndexOf("\n");
            if (indexOfNewline != -1)
            {
                line = line.Substring(0, indexOfNewline);
            }
        }

        if (line.Length > this.MaxCharsPerLine)
        {           
            line = line.Substring(0, this.MaxCharsPerLine - 3);
            line += "...";
        }

        return line;
    }
}
