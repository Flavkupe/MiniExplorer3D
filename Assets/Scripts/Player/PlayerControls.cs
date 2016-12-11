using UnityEngine;
using System.Collections;

public class PlayerControls : MonoBehaviour {

    public Camera cameraView;

    public Texture crosshairTexture;

	// Use this for initialization
	void Start () {
        Cursor.visible = false;
	}
	
	// Update is called once per frame
	void Update () {
        RaycastHit hitInfo;
        Vector3 front = this.cameraView.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f)).direction.normalized;
        if (Physics.Raycast(this.cameraView.transform.position, front, out hitInfo, 50.0f, 1 << 8, QueryTriggerInteraction.Collide))         
        {
            if (hitInfo.collider.GetComponent<ICanLookAt>() != null)
            {
                hitInfo.collider.GetComponent<ICanLookAt>().LookAt(this.gameObject);

                if (Input.GetKeyUp(KeyCode.Space) && hitInfo.collider.GetComponent<ICanLookAtAndInteract>() != null)
                {
                    (hitInfo.collider.GetComponent<ICanLookAtAndInteract>()).InteractWith(this.gameObject, KeyCode.Space);                    
                }
            }
        }                
	}

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(Screen.width / 2, Screen.height / 2, 16, 16), crosshairTexture);
    }

    
}
