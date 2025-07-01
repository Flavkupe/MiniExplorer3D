using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LinkBoard : MonoBehaviour, ICanLookAtAndInteract
{

    public List<LinkedLocationData> links = new List<LinkedLocationData>();

    public float Padding = 0.5f;
    public float AutoSortXInc = 1.0f;
    public float AutoSortYInc = 1.0f;

    public MeshFilter meshFilter;

    public LinkHolder[] PossibleLinkHolders;

    public Transform DirectionPointer; 

    public void SetLinks(List<LinkedLocationData> links)
    {
        if (this.PossibleLinkHolders == null)
        {
            return;
        }

        this.links = links;
        float xScale = meshFilter.transform.localScale.x;
        float yScale = meshFilter.transform.localScale.y;
        float zScale = meshFilter.transform.localScale.z;

        foreach (LinkedLocationData link in links)
        {
            Vector3 localPosition = new Vector3();

            float xGen = Random.Range((meshFilter.mesh.bounds.min.x * xScale) + Padding, (meshFilter.mesh.bounds.max.x * xScale) - Padding);
            float yGen = Random.Range((meshFilter.mesh.bounds.min.y * yScale) + Padding, (meshFilter.mesh.bounds.max.y * yScale) - Padding);
            float zGen = Random.Range((meshFilter.mesh.bounds.min.z * zScale) + Padding, (meshFilter.mesh.bounds.max.z * zScale) - Padding);
            
            localPosition = new Vector3(xGen, yGen, zGen);

            if (DirectionPointer != null)
            {
                //holder.transform.rotation = Quaternion.LookRotation(forward.normalized);

                localPosition = localPosition.SetZ(this.DirectionPointer.localPosition.z);

                // Rotate to face front
                //Vector3 frontPosition = holder.transform.position;
                //frontPosition += forward;
                //holder.transform.LookAt(frontPosition);
                
                
            }

            LinkHolder holder = Instantiate(PossibleLinkHolders.GetRandom());
            holder.transform.parent = this.transform;
            holder.transform.localPosition = localPosition;

            holder.transform.rotation = Quaternion.LookRotation(this.transform.forward, this.transform.up);
            holder.transform.Rotate(this.transform.up, -90);

            holder.SetLink(link);
        }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void LookAt(GameObject source)
    {
        // Nothing
    }

    public bool InteractWith(GameObject source, KeyCode key)
    {
        float xScale = meshFilter.transform.localScale.x;
        float yScale = meshFilter.transform.localScale.y;
        
        float minX = (meshFilter.mesh.bounds.min.x * xScale) + this.Padding;
        float minY = (meshFilter.mesh.bounds.max.y * yScale) - this.Padding;
        float maxX = (meshFilter.mesh.bounds.max.x * xScale) - this.Padding;        
        float x = minX;
        float y = minY;

        LinkHolder[] links = this.GetComponentsInChildren<LinkHolder>();
        if (links != null)
        {
            foreach (LinkHolder link in links)
            {               
                float z = link.transform.localPosition.z;
                link.transform.localPosition = new Vector3(x, y, z);

                x += AutoSortXInc;
                
                if (x > maxX)
                {
                    x = minX;
                    y -= AutoSortYInc;
                }
            }

            return true;
        }
        else
        {
            return false;
        }
    }
}
