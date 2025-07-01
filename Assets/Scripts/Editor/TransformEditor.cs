using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Transform))]
public class TransformInspector : Editor
{

    public bool showTools;
    public bool copyPosition;
    public bool copyRotation;
    public bool copyScale;
    public bool pastePosition;
    public bool pasteRotation;
    public bool pasteScale;
    public bool selectionNullError;

    public override void OnInspectorGUI()
    {

        Transform t = (Transform)target;

        // Replicate the standard transform inspector gui
        EditorGUIUtility.LookLikeControls();
        EditorGUI.indentLevel = 0;
        Vector3 position = EditorGUILayout.Vector3Field("Position", t.localPosition);
        Vector3 eulerAngles = EditorGUILayout.Vector3Field("Rotation", t.localEulerAngles);
        Vector3 scale = EditorGUILayout.Vector3Field("Scale", t.localScale);

        if (t.GetComponent<Collider2D>() != null)
        {
            EditorGUILayout.LabelField("Width", t.GetComponent<Collider2D>().bounds.size.x.ToString());
            EditorGUILayout.LabelField("Height", t.GetComponent<Collider2D>().bounds.size.y.ToString());
        }

        if (GUI.changed)
        {
            SetCopyPasteBools();
            Undo.RecordObject(t, "Transform Change");

            t.localPosition = FixIfNaN(position);
            t.localEulerAngles = FixIfNaN(eulerAngles);
            t.localScale = FixIfNaN(scale);
        }
    }

    private Vector3 FixIfNaN(Vector3 v)
    {
        if (float.IsNaN(v.x))
        {
            v.x = 0;
        }
        if (float.IsNaN(v.y))
        {
            v.y = 0;
        }
        if (float.IsNaN(v.z))
        {
            v.z = 0;
        }
        return v;
    }

    void OnEnable()
    {
        showTools = EditorPrefs.GetBool("ShowTools", false);
        copyPosition = EditorPrefs.GetBool("Copy Position", true);
        copyRotation = EditorPrefs.GetBool("Copy Rotation", true);
        copyScale = EditorPrefs.GetBool("Copy Scale", true);
        pastePosition = EditorPrefs.GetBool("Paste Position", true);
        pasteRotation = EditorPrefs.GetBool("Paste Rotation", true);
        pasteScale = EditorPrefs.GetBool("Paste Scale", true);
    }

    void TransformCopyAll()
    {
        copyPosition = true;
        copyRotation = true;
        copyScale = true;
        GUI.changed = true;
    }

    void TransformCopyNone()
    {
        copyPosition = false;
        copyRotation = false;
        copyScale = false;
        GUI.changed = true;
    }

    void SetCopyPasteBools()
    {
        pastePosition = copyPosition;
        pasteRotation = copyRotation;
        pasteScale = copyScale;

        EditorPrefs.SetBool("Copy Position", copyPosition);
        EditorPrefs.SetBool("Copy Rotation", copyRotation);
        EditorPrefs.SetBool("Copy Scale", copyScale);
        EditorPrefs.SetBool("Paste Position", pastePosition);
        EditorPrefs.SetBool("Paste Rotation", pasteRotation);
        EditorPrefs.SetBool("Paste Scale", pasteScale);
    }
}