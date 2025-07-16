using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

public class DataLogger
{
    private List<LoggingDataObject> dataPoints = new List<LoggingDataObject>();

    public void Sample(LoggingDataObject data)
    {
        dataPoints.Add(data);
    }

    public void Clear()
    {
        dataPoints.Clear();
    }

    // Groups data points by type and outputs each grouped type to a specific file
    // in CSV format.
    public void OutputToFiles()
    {
        if (dataPoints.Count == 0)
        {
            return; // Nothing to output
        }

        // Create a unique batch folder in the temp directory
        string baseDir = Path.Combine(Path.GetTempPath(), "DataLoggerBatches");
        Directory.CreateDirectory(baseDir);
        string batchDir = Path.Combine(baseDir, $"Batch_{DateTime.Now:yyyyMMdd_HHmmssfff}");
        Directory.CreateDirectory(batchDir);

        // Group by DataType
        var groups = dataPoints.GroupBy(dp => dp.DataType);
        foreach (var group in groups)
        {
            string fileName = $"{group.Key}.csv";
            string filePath = Path.Combine(batchDir, fileName);
            using (var writer = new StreamWriter(filePath, false))
            {
                // Write CSV header (use reflection to get property names)
                var first = group.FirstOrDefault();
                if (first != null)
                {
                    var props = first.GetType().GetProperties();
                    writer.WriteLine(string.Join(",", props.Select(p => p.Name)));
                    foreach (var item in group)
                    {
                        var values = props.Select(p =>
                        {
                            var val = p.GetValue(item, null);
                            if (val == null) return "";
                            string s = val.ToString();
                            if (s.Contains(",") || s.Contains("\""))
                                s = $"\"{s.Replace("\"", "\"\"")}"; // Escape quotes and commas
                            return s;
                        });
                        writer.WriteLine(string.Join(",", values));
                    }
                }
            }
        }

        // Open explorer to the batch directory
        try
        {
            Process.Start("explorer.exe", batchDir);
        }
        catch { /* ignore errors */ }
    }
}
