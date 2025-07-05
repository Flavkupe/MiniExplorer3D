using HtmlAgilityPack;
using System;
using UnityEditor.Experimental.GraphView;

public class WikipediaMainPageProcessor : WikipediaBaseProcessor
{
    private Uri currentUri;

    public override void ProcessHtml(MainLocation location, HtmlDocument htmlDoc, Uri currentUri)
    {
        this.currentUri = currentUri;
        location.LocationData.Sections.Clear();

        // Set the location name
        location.Name = "Wikipedia Main Page";
        // Try to extract the <h1> if present
        var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1");
        if (titleNode != null)
        {
            location.Name = HtmlDecode(titleNode.InnerText);
        }

        // Main Page portal sections
        string[] portalIds = new[] { "mp-tfa", "mp-dyk", "mp-itn", "mp-otd", "mp-tfp" };
        foreach (string portalId in portalIds)
        {
            var portalNode = htmlDoc.DocumentNode.SelectSingleNode($"//div[@id='{portalId}']");
            if (portalNode == null)
            {
                continue;
            }
            
            var section = ParsePortalSection(portalNode, portalId);
            if (section != null)
            {
                location.LocationData.Sections.Add(section);
            }
        }
    }

    private SectionData ParsePortalSection(HtmlNode portalNode, string portalId)
    {
        string title = null;

        // Try to get the preceding <h2> node if it exists
        var h2Node = portalNode.SelectSingleNode("preceding-sibling::h2[1]");

        // Try to get the heading (usually the first <h2> or <h3> inside the portal)
        var headingNode = h2Node ?? portalNode.SelectSingleNode(".//h2") ?? portalNode.SelectSingleNode(".//h3");
        if (headingNode != null)
            title = HtmlDecode(headingNode.InnerText);
        else
            title = portalId;

        var section = new SectionData
        {
            Title = title,
            SectionType = SectionType.Standard
        };

        // Only process top-level <p> and <ul>/<ol> tags (direct children)
        foreach (var node in portalNode.ChildNodes)
        {
            if (node.Name == "p")
            {
                string text = HtmlDecode(node.InnerText);
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }
                var textData = new LocationTextData(text);
                ExtractLinks(node, textData, section, this.currentUri.Host);
                section.LocationText.Add(textData);
            }
            else if (node.Name == "ul" || node.Name == "ol")
            {
                HandleListNode(node, section, this.currentUri.Host);
            }
        }

        // Extract images (all descendant images), but skip gifs
        var imgNodes = portalNode.SelectNodes(".//img");
        if (imgNodes != null)
        {
            foreach (var img in imgNodes)
            {
                string src = img.GetAttributeValue("src", "").ToLowerInvariant();
                if (src.EndsWith(".gif"))
                {
                    continue; // skip gifs
                }
                string alt = img.GetAttributeValue("alt", "");
                string imageUrl = Utils.EnsureHttps(Utils.GetImageUrlFromImageTag(img, this.currentUri.Host));
                section.PodiumImages.Add(new ImagePathData(alt, imageUrl));   
            }
        }

        if (section.LinkedLocationData.Count > 0)
        {
            // If the section has any linked locations, create a door link to the first one
            var firstLink = section.LinkedLocationData[0];
            var doorLink = new LinkedLocationData(firstLink.DisplayName, firstLink.Path, LinkedLocationDataType.DoorLink);
            section.LinkedLocationData.Add(doorLink);
        }

        return section;
    }
}
