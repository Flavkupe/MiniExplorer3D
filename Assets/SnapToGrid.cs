using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SnapToGrid : MonoBehaviour 
{
    public float snapValue = 8.0f;
    
    // Update is called once per frame
    void Update () 
    {
        float snapInverse = 1 / snapValue;

        float x, y, z;

        // if snapValue = .5, x = 1.45 -> snapInverse = 2 -> x*2 => 2.90 -> round 2.90 => 3 -> 3/2 => 1.5
        // so 1.45 to nearest .5 is 1.5
        x = Mathf.Round(transform.position.x * snapInverse) / snapInverse;
        y = Mathf.Round(transform.position.y * snapInverse) / snapInverse;
        z = Mathf.Round(transform.position.z * snapInverse) / snapInverse;        

        transform.position = new Vector3(x, y, z);
    }
}
