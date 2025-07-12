

using System.Linq;
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

    public virtual void ReplaceWithUnused()
    {
        this.ClearAssignment();
        var decorPlaceholders = this.GetComponentsInDirectChildren<Placeholder>().Where(a => a.PartType == Placeholder.RoomPartType.Decor).ToList();
        var placeholder = decorPlaceholders.GetRandom();
        var decor = placeholder?.ReplaceInstance<Decor>();
        if (decor == null)
        {
            this.gameObject.SetActive(false);
            return;
        }

        placeholder.gameObject.SetActive(true);
        decor.gameObject.SetActive(true);
        decor.Populate();
    }
}
