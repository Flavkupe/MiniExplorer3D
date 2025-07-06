

using UnityEngine;

public abstract class ExhibitBase : MonoBehaviour, IMatchesPrefab
{
    public SectionType SectionTypes;
    public bool IsAssigned { get; protected set; }

    public string PrefabID
    {
        get
        {

            if (this.transform.parent != null)
            {
                var parent = this.transform.parent.GetComponent<ExhibitBase>();
                if (parent != null)
                {
                    // If this is a subexhibit, use the parent's ID
                    return $"{parent.PrefabID}_{this.name}";
                }
            }

            return this.name;
        }
    }

    public abstract void ClearAssignment();

    public abstract bool CanHandleSection(SectionData section);

    public abstract void PopulateExhibit(ExhibitData data);

    public abstract RatingResult RateSectionMatch(SectionData section);

    public virtual void PopulateParts()
    {
    }
}