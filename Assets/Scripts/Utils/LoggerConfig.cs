using UnityEngine;

public class LoggerConfig : MonoBehaviour {
    public static LoggerConfig Instance { get; private set; }

    [Header("Logger Filters")]
    public bool LogRatings = false;
    public bool LogOther = false;

    private void Awake() {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
}
