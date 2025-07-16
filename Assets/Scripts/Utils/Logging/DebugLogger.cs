using UnityEngine;

public enum LoggerFilter {
    LogRatings,
    LogOther,
}

public static class DebugLogger {
    private static DataLogger dataLogger = new DataLogger();

    public static void LogSample(LoggingRatingData data)
    {
        var config = LoggerConfig.Instance;
        if (config == null || !config.LogRatings)
        {
            return;
        }

        dataLogger.Sample(data);
    }

    public static void LogSample(LoggingRoomRatingData data)
    {
        var config = LoggerConfig.Instance;
        if (config == null || !config.LogRatings)
        {
            return;
        }

        dataLogger.Sample(data);
    }

    public static void GenerateSampleCsvs()
    {
        dataLogger.OutputToFiles();
        dataLogger.Clear();
    }

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
