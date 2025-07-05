


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