

using System.Linq;
using UnityEngine;

public class ExhibitPlaceholder : ExhibitBase
{
    public Exhibit[] Exhibits = new Exhibit[] { };

    private Exhibit pickedExhibit = null;

    public GameObject Cube;

    public override bool CanHandleSection(SectionData section)
    {
        return Exhibits.Any(exhibit => exhibit.CanHandleSection(section));
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

        pickedExhibit = GetInstance(pickedExhibit);

        pickedExhibit.PopulateExhibit(data);
    }

    public override RatingResult RateSectionMatch(SectionData section)
    {
        return Exhibits.Max(a => a.RateSectionMatch(section));
    }

    public Exhibit GetInstance(Exhibit exhibit)
    {
        // TODO: figure out rotation
        Exhibit newInstance = Instantiate(exhibit);
        newInstance.transform.SetParent(this.transform, true);
        newInstance.transform.localPosition = Vector3.zero;
        newInstance.transform.localRotation = Quaternion.identity;
        return newInstance;
    }
}
