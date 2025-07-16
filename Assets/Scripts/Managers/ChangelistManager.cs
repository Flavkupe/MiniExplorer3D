using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;

[Serializable]
public class Changelist
{
    public List<ChangelistSection> sections;
}

[Serializable]
public class ChangelistSection
{
    public string version;
    public string date;
    public string title;
    public List<string> entries;
}

public class ChangelistManager : MonoBehaviour
{
    public string changelistFileName = "changelist.json";
    public Changelist Changelist { get; private set; }

    public GameObject Container;

    public TextMeshProUGUI TextPrefab;

    public static ChangelistManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PopulateChangelistUI()
    {
        if (Container == null || TextPrefab == null || Changelist == null)
            return;

        // Remove existing children
        foreach (Transform child in Container.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var section in Changelist.sections)
        {
            // Header for section
            var header = Instantiate(TextPrefab, Container.transform);
            header.text = $"<b>{section.version}</b>  <i>{section.date}</i>\n{section.title}";
            header.fontSize = 20;

            // Entries
            if (section.entries != null)
            {
                foreach (var entry in section.entries)
                {
                    var entryText = Instantiate(TextPrefab, Container.transform);
                    entryText.text = $"- {entry}";
                    entryText.fontSize = 14;
                }
            }
        }
    }

    public Changelist LoadChangelist()
    {
        var path = Path.Combine("Changelist", Path.GetFileNameWithoutExtension(changelistFileName));
        TextAsset jsonAsset = Resources.Load<TextAsset>(path);
        if (jsonAsset == null)
        {
            Debug.LogError($"Changelist JSON file not found: {changelistFileName}");
            return null;
        }
        Changelist = JsonConvert.DeserializeObject<Changelist>(jsonAsset.text);
        PopulateChangelistUI();
        return Changelist;
    }
}
