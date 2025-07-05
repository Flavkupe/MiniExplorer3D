using System.Collections.Generic;

public class ExhibitData : IMatchesPrefab
{
    public string PrefabID { get; set; }
    public SectionData SectionData { get; set; }
    public List<ExhibitData> SubExhibitData { get; private set; } = new List<ExhibitData>();

    public bool IsAssigned => SectionData != null;

    public ExhibitData() { }
    public ExhibitData(string prefabID, SectionData sectionData)
    {
        this.PrefabID = prefabID;
        this.SectionData = sectionData;
        foreach (var subsection in sectionData.Subsections)
        {
            SubExhibitData.Add(new ExhibitData(prefabID, subsection));
        }
    }
}
