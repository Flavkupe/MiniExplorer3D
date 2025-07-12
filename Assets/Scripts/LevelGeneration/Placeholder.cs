using System.Linq;
using UnityEngine;

public class Placeholder : MonoBehaviour , ICanSupportTitle
{
    public enum RoomPartType
    {
        Door,
        Reading,
        ImageFrame,
        TableOfContentsPodium,
        Decor,
    }

    public RoomPartType PartType;

    [Tooltip("The slab object for the placeholder, which will be hidden when the placeholder is applied.")]
    public GameObject PlaceholderObject;

    public bool CanHandleText => this.PartType == RoomPartType.Reading;
    public bool CanHandleImage => this.PartType == RoomPartType.ImageFrame;

    public bool SupportsTitle => this.transform.GetComponentsInDirectChildren<ICanSupportTitle>().Any(c => c.SupportsTitle);

    private T GetPlaceholderOfType<T, R>(RoomPartType partType) where T : MonoBehaviour where R : MonoBehaviour
    {
        var mainComponents = this.transform.GetComponentsInDirectChildren<R>().ToList();
        if (mainComponents == null || mainComponents.Count == 0)
        {
            Debug.LogWarning($"No items of {partType} found for Placeholder {this.name}");
            return null;
        }

        var random = mainComponents.GetRandom();

        var instance = random.GetComponentInChildren<T>();
        if (instance == null)
        {
            Debug.LogWarning($"Could not cast part in Placeholder {this.name} to {(typeof(T))}");
            return null;
        }

        return instance;
    }

    public T ReplaceInstance<T>() where T : MonoBehaviour
    {
        this.PlaceholderObject.gameObject.SetActive(false);
        switch (this.PartType)
        {
            case RoomPartType.Reading:
                return GetPlaceholderOfType<T, ReadingContent>(RoomPartType.Reading);
            case RoomPartType.ImageFrame:
                return GetPlaceholderOfType<T, RoomImageFrame>(RoomPartType.ImageFrame);
            case RoomPartType.Door:
                return GetPlaceholderOfType<T, Door>(RoomPartType.Door);
            case RoomPartType.Decor:
                return GetPlaceholderOfType<T, Decor>(RoomPartType.Decor);
            default:
                Debug.LogWarning($"Unsupported placeholder type for Placeholder {this.name}");
                return null;
        }
    }
}
