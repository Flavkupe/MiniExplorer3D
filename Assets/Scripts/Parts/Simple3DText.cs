
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
public class Simple3DText : MonoBehaviour
{
    public TextMesh childTextMesh;

    public int MaxWordsPerRow = 7;
    private string text;

    void Start()
    {        
    }

    void Update()
    {
    }

    public void SetText(string newText)
    {
        StringBuilder builder = new StringBuilder();
        List<string> words = newText.Trim().Split(' ').ToList();

        int counter = 0;
        foreach (string word in words)
        {
            if (counter > MaxWordsPerRow)
            {
                counter = 0;
                builder.AppendLine();
            }

            builder.Append(word + " ");
            counter++;
        }

        this.text = builder.ToString(); ;

        if (this.childTextMesh != null)
        {
            this.childTextMesh.text = this.text;
        }
    }

    public void ToggleText(bool toggle)
    {
        if (this.childTextMesh)
        {
            this.childTextMesh.gameObject.SetActive(toggle);
        }
    }
}

