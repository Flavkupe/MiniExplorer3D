using UnityEngine;
using System.Collections;

public class PlayerControls : MonoBehaviour
{
    public bool DebugMode = false;

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
        if (WindowManager.IsAnyWindowOpen())
        {
            return; // Do not interact while windows are open
        }

        RaycastHit hitInfo;
        Vector3 front = this.cameraView.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f)).direction.normalized;

        DebugLogRaycastInfo("<none>");

        if (DebugMode)
        {
            Debug.DrawRay(cameraView.transform.position, front * 50.0f, Color.blue, 0.1f);
        }

        if (!Physics.Raycast(this.cameraView.transform.position, front, out hitInfo, 50.0f, 1 << 8, QueryTriggerInteraction.Collide))
        {
            return;
        }

        var colliderName = hitInfo.collider?.gameObject?.name ?? "<null>";
        DebugLogRaycastInfo(colliderName);

        var collider = hitInfo.collider.GetComponentInParent<ICanLookAt>();
        if (collider == null)
        {
            DebugLogRaycastInfo($"{colliderName} (non-viewable)");
            return;
        }

        collider.LookAt(this.gameObject);

        var interactable = hitInfo.collider.GetComponentInParent<ICanLookAtAndInteract>();
        if (interactable == null)
        {
            DebugLogRaycastInfo($"{collider.Name} (non-interactable)");
            return;
        }

        DebugLogRaycastInfo($"{interactable.Name} (interactable)");
        currentTexture = inspectTexture;
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetMouseButton(0))
        {
            interactable.InteractWith(this.gameObject, KeyCode.Space);
        }
    }

    private void DebugLogRaycastInfo(string message)
    {
        if (DebugMode)
        {
            DevWindow.Instance?.SetRaycastInfo(message);
        }
    }

    void OnGUI()
    {
        if (WindowManager.IsAnyWindowOpen())
        {
            return; // Do not draw crosshair if any window is open
        }

        GUI.DrawTexture(new Rect(Screen.width / 2, Screen.height / 2, 16, 16), currentTexture);
    }    
}
