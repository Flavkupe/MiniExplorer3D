using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExhibitPlaceholder : ExhibitBase
{
    private List<ExhibitBase> Exhibits
    {
        get
        {
            return new List<ExhibitBase>(this.transform.GetComponentsInDirectChildren<ExhibitBase>());
        }
    }
    
    private ExhibitBase pickedExhibit = null;

    public GameObject Cube;

    public override bool CanHandleSection(SectionData section)
    {
        foreach (var exhibit in Exhibits)
        {
            if (exhibit == null)
            {
                Debug.LogError($"Null Exhibit in {this.PrefabID}");
                continue;
            }

            if (exhibit.CanHandleSection(section))
            {
                return true;
            }
        }
        return false;
    }

    public override void ClearAssignment()
    {
        // no-op
        return;
    }

    public override void PopulateExhibit(ExhibitData data)
    {
        if (pickedExhibit == null)
        {
            pickedExhibit = Exhibits.MaxValue(a => a.RateSectionMatch(data.SectionData).Score);
        }

        if (Cube != null)
        {
            Cube.SetActive(false);
        }

        // Just a failsafe
        if (!pickedExhibit.CanHandleSection(data.SectionData))
        {
            Debug.LogError("ExhibitPlaceholder cannot handle the section: " + data.SectionData);
            return;
        }

        pickedExhibit.gameObject.SetActive(true);
        pickedExhibit.PopulateExhibit(data);
    }

    public override RatingResult RateSectionMatch(SectionData section)
    {
        return Exhibits.Max(a => a.RateSectionMatch(section));
    }

    public override void ReplaceWithUnused()
    {
        base.ReplaceWithUnused();

        if (this.Cube != null)
        {
            this.Cube.SetActive(false);
        }
    }
}
