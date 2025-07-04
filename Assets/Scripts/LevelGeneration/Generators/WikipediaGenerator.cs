using System;
using UnityEngine;
using HtmlAgilityPack;

public class WikipediaGenerator : WebLevelGenerator
{
    // Private fields for shared state
    private SectionData leadSection;
    private SectionData currentSection;
    private SectionData currentSubsection;
    private bool foundFirstH2;
    private MainLocation currentLocation;
    private Uri currentUri;

    protected override void ProcessHtmlDocument(MainLocation location, Uri currentUri)
    {
        this.currentLocation = location;
        this.currentUri = currentUri;
        this.leadSection = new SectionData { SectionType = SectionType.Main };
        this.currentSection = null;
        this.currentSubsection = null;
        this.foundFirstH2 = false;

        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(location.LocationData.RawData);

        HtmlNode titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1");
        if (titleNode != null)
        {
            location.Name = this.HtmlDecode(titleNode.InnerText);
            this.leadSection.Title = location.Name;
            if (StageManager.CurrentLocation.Path == location.Path)
            {
                StageManager.CurrentLocation.Name = location.Name;
            }
        }

        HtmlNode contentNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='mw-content-text']//div[contains(@class, 'mw-parser-output')]");
        if (contentNode == null)
        {
            Debug.LogWarning("Content node not found in Wikipedia page.");
            return;
        }

        location.LocationData.Sections.Clear();

        foreach (var node in contentNode.ChildNodes)
        {
            if (node.Name == "h2")
            {
                this.HandleH2Node(node);
            }
            else if (node.Name == "div" && node.GetAttributeValue("class", "").Contains("mw-heading"))
            {
                // example: <div class="mw-heading mw-heading2"><h2 id="Taxonomy">Taxonomy</h2></div>
                var h2 = node.SelectSingleNode("h2");
                if (h2 != null)
                {
                    this.HandleH2Node(h2);
                    continue;
                }
            }
            else if (node.Name == "h3")
            {
                this.HandleH3Node(node);
            }
            else if (node.Name == "p")
            {
                this.HandlePNode(node);
            }
            else if (node.Name == "table")
            {
                this.HandleTableNode();
            }
            else if (node.Name == "div" && node.GetAttributeValue("class", "") == "thumbinner")
            {
                this.HandleThumbinnerDivNode(node);
            }
        }
        if (this.leadSection.LocationText.Count > 0 || this.leadSection.ImagePaths.Count > 0)
        {
            location.LocationData.Sections.Insert(0, this.leadSection);
        }
    }

    private void HandleH2Node(HtmlNode node)
    {
        this.foundFirstH2 = true;
        string title = string.Empty;
        string anchor = string.Empty;
        // Try to find span.mw-headline for anchor and title, fallback to h2's own text/id
        var headline = node.SelectSingleNode("span[@class='mw-headline']");
        if (headline != null)
        {
            title = this.HtmlDecode(headline.InnerText);
            anchor = headline.GetAttributeValue("id", "");
        }
        else
        {
            title = this.HtmlDecode(node.InnerText);
            anchor = node.GetAttributeValue("id", "");
        }
        this.currentSection = new SectionData {
            Title = title,
            Anchor = anchor,
            SectionType = SectionType.Standard
        };
        this.currentLocation.LocationData.Sections.Add(this.currentSection);
        this.currentSubsection = null;
    }

    private void HandleH3Node(HtmlNode node)
    {
        if (this.currentSection == null)
        {
            Debug.LogWarning("Encountered h3 node without a preceding h2 section.");
            return;
        }

        var headline = node.SelectSingleNode("span[@class='mw-headline']");
        string title = headline != null ? this.HtmlDecode(headline.InnerText) : "";
        string anchor = headline != null ? headline.GetAttributeValue("id", "") : "";
        this.currentSubsection = new SectionData {
            Title = title,
            Anchor = anchor,
            SectionType = SectionType.Standard
        };
        this.currentSection.Subsections.Add(this.currentSubsection);
    }

    private void HandlePNode(HtmlNode node)
    {
        var text = this.HtmlDecode(node.InnerText);
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var textData = new LocationTextData(text);
        HtmlNodeCollection linkNodes = node.SelectNodes("a");
        if (linkNodes != null)
        {
            foreach (HtmlNode link in linkNodes)
            {
                string name = this.HtmlDecode(link.InnerText);
                string href = link.GetAttributeValue("href", "");
                string url = this.EnsureHttps("https://" + this.currentUri.Host + "/" + href.TrimStart('/'));
                textData.LinkedLocationData.Add(new LinkedLocationData(name, url));
            }
        }
        if (!this.foundFirstH2)
            this.leadSection.LocationText.Add(textData);
        else if (this.currentSubsection != null)
            this.currentSubsection.LocationText.Add(textData);
        else if (this.currentSection != null)
            this.currentSection.LocationText.Add(textData);
    }

    private void HandleTableNode()
    {
        if (this.currentSubsection != null)
            this.currentSubsection.SectionType = SectionType.Table;
        else if (this.currentSection != null)
            this.currentSection.SectionType = SectionType.Table;
    }

    private void HandleThumbinnerDivNode(HtmlNode node)
    {
        HtmlNode imgTag = node.SelectSingleNode("a/img");
        HtmlNode caption = node.SelectSingleNode("div[@class='thumbcaption']");
        if (caption != null && imgTag != null)
        {
            string imageCaption = this.HtmlDecode(caption.InnerText);
            string imageUrl = this.EnsureHttps(this.GetImageUrlFromImageTag(imgTag, this.currentUri.Host));
            var imageData = new ImagePathData(imageCaption, imageUrl);
            if (!this.foundFirstH2)
                this.leadSection.ImagePaths.Add(imageData);
            else if (this.currentSubsection != null)
                this.currentSubsection.ImagePaths.Add(imageData);
            else if (this.currentSection != null)
                this.currentSection.ImagePaths.Add(imageData);
        }
    }

    private string HtmlDecode(string text)
    {
        return string.IsNullOrEmpty(text) ? string.Empty : HtmlEntity.DeEntitize(text).Trim();
    }
}
