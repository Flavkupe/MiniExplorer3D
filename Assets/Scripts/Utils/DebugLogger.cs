using UnityEngine;

public enum LoggerFilter {
    LogRatings,
    LogOther,
}

public static class DebugLogger {
    public static void Log(string message, LoggerFilter filter) {
        var config = LoggerConfig.Instance;
        if (config == null)
        {
            return;
        }

        switch (filter) {
            case LoggerFilter.LogRatings:
                if (config.LogRatings)
                {
                    Debug.Log(message);
                }
                break;
            case LoggerFilter.LogOther:
                if (config.LogOther)
                {
                    Debug.Log(message);
                }
                break;
            default:
                // no-op
                break;
        }
    }
}
