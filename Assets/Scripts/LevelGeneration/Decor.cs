
using System.Linq;
using UnityEngine;

public class Decor : MonoBehaviour
{
    public void Populate()
    {
        var decorPlaceholders = this.GetComponentsInDirectChildren<Placeholder>().Where(a => a.PartType == Placeholder.RoomPartType.Decor).ToList();
        foreach (var placeholder in decorPlaceholders)
        {
            var instance = placeholder.ReplaceInstance<Decor>();
            if (instance == null)
            {
                continue;
            }

            placeholder.gameObject.SetActive(true);
            instance.gameObject.SetActive(true);
        }        
    }
}