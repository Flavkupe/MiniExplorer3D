using HtmlAgilityPack;
using System;
using System.Collections.Generic;

public static class Utils
{
    public static string EnsureHttps(string url)
    {
        if (string.IsNullOrEmpty(url)) return url;
        if (url.StartsWith("http://"))
            return "https://" + url.Substring(7);
        if (url.StartsWith("//"))
            return "https:" + url;
        return url;
    }

    public static string GetImageUrlFromImageTag(HtmlNode node, string currentUriHost)
    {
        string imageSrc = node.GetAttributeValue("src", "");
        if (imageSrc.StartsWith("//"))
        {
            return "https://" + imageSrc.TrimStart('/');
        }
        else
        {
            return "https://" + currentUriHost + "/" + imageSrc.TrimStart('/');
        }
    }
}


public class RatingResult<T> : RatingResult where T: class
{
    public static new RatingResult<T> NoMatch { get; } = new RatingResult<T>(0, null, false);

    public T Match { get; private set; }

    public RatingResult(float score, T data, bool isValid = true)
        : base(score, isValid)
    {
        Match = data;
    }
}

/// <summary>
/// A match result between a section and some prefab (such as an exhibit).
/// </summary>
public class RatingResultMatch
{
    public SectionData SectionData { get; private set; }
    public string PrefabID { get; private set; }

    public RatingResultMatch(SectionData sectionData, string prefabID)
    {
        SectionData = sectionData;
        PrefabID = prefabID;
    }
}

public class RatingResult : IComparable<RatingResult>
{
    public float Score { get; set; }
    public bool IsValid { get; set; }

    /// <summary>
    /// List of sections matched for this result.
    /// </summary>
    public List<RatingResultMatch> MatchedSections = new();

    public static RatingResult NoMatch { get; } = new RatingResult(0, false);

    public RatingResult(float score, bool isValid = true)
    {
        Score = score;
        IsValid = isValid;
    }

    public int CompareTo(RatingResult other)
    {
        return this.Score.CompareTo(other.Score);
    }
}
