using System;

[Serializable]
public class ExhibitData : IMatchesPrefab
{
    public string PrefabID { get; set; }
    public SectionData SectionData { get; set; }

    public bool IsAssigned => SectionData != null;

    public ExhibitData() { }
    public ExhibitData(string prefabID, SectionData sectionData)
    {
        this.PrefabID = prefabID;
        this.SectionData = sectionData;
    }
}
