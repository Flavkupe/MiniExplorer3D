using System.Runtime.CompilerServices;
using UnityEngine;

public class DevWindow : BaseWindow
{
    public TMPro.TMP_Text errorLogText;
    public TMPro.TMP_Text raycastInfoText;

    private string rayCastText = string.Empty;

    private bool isOpen = false;
    private System.Text.StringBuilder logBuilder = new System.Text.StringBuilder();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        Toggle(false);
    }

    private static DevWindow instance = null;
    public static DevWindow Instance => instance;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote)) // Tilde key (~)
        {
            Toggle(!isOpen);
        }
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        logBuilder.AppendLine(logString);
        if (type == LogType.Error || type == LogType.Exception)
        {
            logBuilder.AppendLine(stackTrace);
        }
        logBuilder.AppendLine(); // Add extra newline for spacing

        if (errorLogText != null)
        {
            errorLogText.text = logBuilder.ToString();
        }
    }

    // Call this to update the error log text
    public void SetErrorLog(string log)
    {
        if (errorLogText != null)
        {
            errorLogText.text = log;
        }
    }

    // Call this to update the raycast info text
    public void SetRaycastInfo(string info)
    {
        rayCastText = info;
    }

    private void FixedUpdate()
    {
        if (raycastInfoText != null)
        {
            raycastInfoText.text = rayCastText;
        }
    }
}
