using UnityEngine;
using System.Collections;

public class PlayerControls : MonoBehaviour {

    public Camera cameraView;

    public Texture crosshairTexture;

    public Texture inspectTexture;

    private Texture currentTexture;

    // Use this for initialization
    void Start () {
        currentTexture = crosshairTexture;
        Cursor.visible = false;
	}
	
	// Update is called once per frame
	void Update () {
        currentTexture = crosshairTexture;
        RaycastToInteractables();

    }

    private void RaycastToInteractables()
    {
        RaycastHit hitInfo;
        Vector3 front = this.cameraView.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f)).direction.normalized;
        if (!Physics.Raycast(this.cameraView.transform.position, front, out hitInfo, 50.0f, 1 << 8, QueryTriggerInteraction.Collide))
        {
            return;
        }

        if (hitInfo.collider.GetComponent<ICanLookAt>() == null)
        {
            return;
        }
        
        hitInfo.collider.GetComponent<ICanLookAt>().LookAt(this.gameObject);

        if (hitInfo.collider.GetComponent<ICanLookAtAndInteract>() != null)
        {
            currentTexture = inspectTexture;
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetMouseButton(0))
            {
                (hitInfo.collider.GetComponent<ICanLookAtAndInteract>()).InteractWith(this.gameObject, KeyCode.Space);
            }
        }
    }

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(Screen.width / 2, Screen.height / 2, 16, 16), currentTexture);
    }    
}
