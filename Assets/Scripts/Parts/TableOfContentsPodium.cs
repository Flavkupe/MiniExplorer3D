using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class TableOfContentsPodium : MonoBehaviour 
{

    private TableOfContents toc;

    public ClickableText modelText;

    public int LinesOfContents;

    public float LineHeight;

    public int MaxCharsPerLine;

    public float ModelWidth = 2.86f;

    public void SetTableOfContents(TableOfContents toc)
    {        
        this.toc = toc;
        if (this.modelText == null || this.modelText.GetComponentInChildren<TextMesh>() == null)
        {
            return;
        }

        if (this.toc.TocItems.Count > LinesOfContents)
        {
            TableOfContents remainder = new TableOfContents();
            remainder.TocItems.AddRange(this.toc.TocItems.GetRange(LinesOfContents, this.toc.TocItems.Count - LinesOfContents));
            this.toc.TocItems.RemoveRange(LinesOfContents, this.toc.TocItems.Count - LinesOfContents);
            this.SpawnNewPodiumAndSplit(remainder);            
        }

        int currentLine = 0;
        foreach (TableOfContents.TOCItem item in this.toc.TocItems)
        {
            ClickableText newText = Instantiate(modelText);            
            string indentation = item.Indentation == 0 ? "" : "  ";
            string line = indentation + item.Rank + "  " + item.Name;

            if (line.Length > this.MaxCharsPerLine)
            {
                line = line.Substring(0, this.MaxCharsPerLine - 3);
                line += "...";
            }

            Action<object> onClick = (object tocItem) =>
            {
                if (tocItem is TableOfContents.TOCItem)
                {
                    StageManager.AttemptTransitionToAnchor((tocItem as TableOfContents.TOCItem).Anchor);
                }
                
            };

            newText.SetText(line, onClick, item);

            
            newText.transform.parent = modelText.transform.parent;
            newText.transform.localRotation = modelText.transform.localRotation;
            newText.transform.localScale = modelText.transform.localScale;
            float offset = (currentLine * this.LineHeight);

            newText.transform.localPosition = modelText.transform.localPosition;
            newText.transform.Translate(Vector3.up * -offset, Space.Self);
            ++currentLine;            
        }
    }

    private void SpawnNewPodiumAndSplit(TableOfContents toTransfer)
    {
        TableOfContentsPodium newPodium = Instantiate(this);
        
        newPodium.transform.rotation = this.transform.rotation;        
        newPodium.transform.parent = this.transform.parent;
        newPodium.transform.position = this.transform.position + (transform.right * ModelWidth);
        
        newPodium.SetTableOfContents(toTransfer);
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
