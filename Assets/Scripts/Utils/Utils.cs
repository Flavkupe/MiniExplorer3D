using HtmlAgilityPack;

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

public class RatingResult
{
    public float Score { get; set; }
    public bool IsValid { get; set; }
    public static RatingResult NoMatch { get; } = new RatingResult(0, false);

    public RatingResult(float score, bool isValid = true)
    {
        Score = score;
        IsValid = isValid;
    }
}