using UnityEngine;
using System.Collections;

public class Placeholder : MonoBehaviour 
{
    public enum RoomPartType
    {
        Door,
        Reading,
        ImageFrame,
        DisplayPodium,
        TableOfContentsPodium,
        TextPodium,
    }

    public RoomPartType PartType;

    public GameObject[] PossibleItems;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public T GetInstance<T>() where T : MonoBehaviour
    {
        if (this.PossibleItems == null || this.PossibleItems.Length == 0)            
        {
            Debug.LogError("No PossibleItems provided for Placeholder.");
            return null;
        }

        GameObject newInstance = Instantiate(this.PossibleItems.GetRandom());
        T component = newInstance.GetComponent<T>();
        if (component == null)
        {
            component = newInstance.GetComponentInChildren<T>(true);
            if (component == null)
            {
                Destroy(newInstance);
                Debug.LogError("Trying to instantiate type but possible object does not have that type");
                return null;
            }
        }

        newInstance.transform.position = this.transform.position;
        newInstance.transform.rotation = this.transform.rotation;
        newInstance.transform.parent = this.transform.parent;
        Destroy(this.gameObject);
        return component;
    }
}
