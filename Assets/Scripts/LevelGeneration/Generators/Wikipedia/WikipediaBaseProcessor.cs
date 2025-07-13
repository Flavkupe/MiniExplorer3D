using HtmlAgilityPack;
using System.Collections.Generic;

public abstract class WikipediaBaseProcessor
{
    public abstract void ProcessHtml(MainLocation location, HtmlDocument htmlDoc);

    protected void ExtractLinks(HtmlNode node, LocationTextData textData, SectionData section)
    {
        var linkNodes = node.SelectNodes(".//a");
        if (linkNodes != null)
        {
            foreach (var link in linkNodes)
            {
                string name = HtmlEntity.DeEntitize(link.InnerText).Trim();
                string href = link.GetAttributeValue("href", "");
                if (!string.IsNullOrEmpty(href) && href.StartsWith("/wiki/"))
                {
                    // Extract just the article name from the href
                    string articleName = href.Substring("/wiki/".Length);
                    textData.LinkedLocationData.Add(new LinkedLocationData(name, articleName));
                    section.LinkedLocationData.Add(new LinkedLocationData(name, articleName, LinkedLocationDataType.TextLink));
                }
            }
        }
    }

    protected void HandleListNode(HtmlNode node, SectionData section)
    {
        var items = new List<LocationTextData>();
        foreach (var li in node.SelectNodes("li") ?? new HtmlNodeCollection(node))
        {
            string text = HtmlEntity.DeEntitize(li.InnerText).Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                var textData = new LocationTextData(text);
                ExtractLinks(li, textData, section);
                items.Add(textData);
            }
        }
        if (items.Count > 0)
        {
            section.Lists.Add(new ListItemsData(items));
        }
    }

    protected string HtmlDecode(string text)
    {
        return string.IsNullOrEmpty(text) ? string.Empty : HtmlEntity.DeEntitize(text).Trim();
    }
}
