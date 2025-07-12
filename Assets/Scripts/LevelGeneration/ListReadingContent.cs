

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ListReadingContent : MonoBehaviour, ICanSupportTitle, ICanLookAtAndInteract
{
    [Tooltip("The list of reading content to display.")]
    public List<ReadingContent> Readings { get; private set; } = new();
    public bool CanHandleText => true;
    public bool CanHandleImage => false;
    public bool SupportsTitle => false;

    private ListItemsData listData;

    private string combinedText;

    public string Name => this.name;

    public void PopulateParts()
    {
        this.Readings = this.transform.GetComponentsInDirectChildren<ReadingContent>().ToList();
        if (this.Readings == null || this.Readings.Count == 0)
        {
            Debug.LogWarning($"No readings found in {this.name}");
        }
    }

    public void SetList(ListItemsData listData)
    {
        this.listData = listData;
        List<string> strings = new List<string>();
        foreach (var data in listData.Items)
        {
            strings.Add(data.Text);
        }

        this.combinedText = string.Join("\n", strings);

        this.PopulateParts();
        for (var i = 0; i < this.Readings.Count; i++)
        {
            var reading = this.Readings[i];
            if (reading == null)
            {
                Debug.LogWarning($"Reading at index {i} is null in {this.name}");
                continue;
            }

            if (i < listData.Items.Count)
            {
                var readingData = listData.Items[i];
                reading.AddText(readingData);
                reading.gameObject.SetActive(true);
            }
            else
            {
                reading.gameObject.SetActive(false);
            }
        }
    }

    public bool InteractWith(GameObject source, KeyCode key)
    {
        InteractionWindow.Instance.SetText(this.combinedText, true);
        return true;
    }

    public void LookAt(GameObject source)
    {
    }
}