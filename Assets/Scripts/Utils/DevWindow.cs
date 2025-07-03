using UnityEngine;

public class DevWindow : MonoBehaviour
{
    public TMPro.TMP_Text errorLogText;

    public GameObject logWindowRoot;

    private bool isOpen = false;
    private System.Text.StringBuilder logBuilder = new System.Text.StringBuilder();

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void Awake()
    {
        Toggle(false);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote)) // Tilde key (~)
        {
            Toggle(!isOpen);
        }
    }

    private void Toggle(bool enabled)
    {
        isOpen = enabled;
        if (logWindowRoot != null)
        {
            logWindowRoot.SetActive(isOpen);
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
}
