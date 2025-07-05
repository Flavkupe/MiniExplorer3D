using HtmlAgilityPack;
using System;
using UnityEngine;

public class WikipediaArticleProcessor : WikipediaBaseProcessor
{
    // Private fields for shared state
    private SectionData leadSection;
    private SectionData currentSection;
    private SectionData parentSection;
    private bool foundFirstH2;
    private Uri currentUri;

    public override void ProcessHtml(MainLocation location, HtmlDocument htmlDoc, Uri currentUri)
    {
        this.currentUri = currentUri;
        this.leadSection = new SectionData { SectionType = SectionType.Main };
        this.currentSection = null;
        this.foundFirstH2 = false;

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
                this.HandleH2Node(node, location);
            }
            else if (node.Name == "div" && node.GetAttributeValue("class", "").Contains("mw-heading2"))
            {
                var h2 = node.SelectSingleNode("h2");
                if (h2 != null)
                {
                    this.HandleH2Node(h2, location);
                    continue;
                }
            }

            else if (node.Name == "div" && node.GetAttributeValue("class", "").Contains("mw-heading3"))
            {
                var h3 = node.SelectSingleNode("h3");
                if (h3 != null)
                {
                    this.HandleH3Node(h3, location);
                    continue;
                }
            }
            else if (node.Name == "p")
            {
                this.HandlePNode(node);
            }
            else if (node.Name == "ul" || node.Name == "ol")
            {
                this.HandleListNode(node);
            }
            else if (node.Name == "table")
            {
                this.HandleTableNode();
            }
            else if (node.Name == "div" && node.GetAttributeValue("class", "") == "thumbinner")
            {
                this.HandleThumbinnerDivNode(node);
            }
            else if (node.Name == "figure")
            {
                this.HandleFigureNode(node);
            }
            else if ((node.Name == "div" && node.GetAttributeValue("class", "").Contains("gallery")) ||
                        (node.Name == "ul" && node.GetAttributeValue("class", "").Contains("gallery")))
            {
                this.HandleGalleryNode(node);
            }
        }
        if (this.leadSection.LocationText.Count > 0 || this.leadSection.ImagePaths.Count > 0)
        {
            location.LocationData.Sections.Insert(0, this.leadSection);
        }
    }

    private void HandleH2Node(HtmlNode node, MainLocation location)
    {
        this.foundFirstH2 = true;
        string title = string.Empty;
        string anchor = string.Empty;
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
        this.currentSection = new SectionData
        {
            Title = title,
            Anchor = anchor,
            SectionType = SectionType.Standard
        };
        this.parentSection = this.currentSection;
        location.LocationData.Sections.Add(this.currentSection);
    }

    private void HandleH3Node(HtmlNode node, MainLocation location)
    {
        string title = string.Empty;
        string anchor = string.Empty;
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

        var subsection = new SectionData
        {
            Title = title,
            Anchor = anchor,
            SectionType = SectionType.Subsection
        };

        if (this.parentSection == null)
        {
            Debug.LogWarning("Parent section is null when trying to add a subsection. This may indicate an issue with the HTML structure.");
            return;
        }

        this.parentSection.Subsections.Add(subsection);
        this.currentSection = subsection;
    }

    private void HandlePNode(HtmlNode node)
    {
        var text = this.HtmlDecode(node.InnerText);
        if (string.IsNullOrEmpty(text))
        {
            return;
        }
        var textData = new LocationTextData(text);
        ExtractLinks(node, textData, (!this.foundFirstH2 || this.currentSection == null) ? this.leadSection : this.currentSection, this.currentUri.Host);
        if (!this.foundFirstH2 || this.currentSection == null)
            this.leadSection.LocationText.Add(textData);
        else
            this.currentSection.LocationText.Add(textData);
    }

    private void HandleListNode(HtmlNode node)
    {
        if (!this.foundFirstH2 || this.currentSection == null)
            HandleListNode(node, this.leadSection, this.currentUri.Host);
        else
            HandleListNode(node, this.currentSection, this.currentUri.Host);
    }

    private void HandleTableNode()
    {
        if (this.currentSection != null)
            this.currentSection.SectionType = SectionType.Table;
    }

    private void HandleThumbinnerDivNode(HtmlNode node)
    {
        HtmlNode imgTag = node.SelectSingleNode("a/img");
        HtmlNode caption = node.SelectSingleNode("div[@class='thumbcaption']");
        if (caption != null && imgTag != null)
        {
            string imageCaption = this.HtmlDecode(caption.InnerText);
            string imageUrl = Utils.EnsureHttps(Utils.GetImageUrlFromImageTag(imgTag, this.currentUri.Host));
            var imageData = new ImagePathData(imageCaption, imageUrl);
            if (!this.foundFirstH2 || this.currentSection == null)
                this.leadSection.ImagePaths.Add(imageData);
            else
                this.currentSection.ImagePaths.Add(imageData);
        }
    }

    private void HandleFigureNode(HtmlNode node)
    {
        var imgTag = node.SelectSingleNode(".//img");
        var captionNode = node.SelectSingleNode(".//figcaption");
        if (imgTag != null)
        {
            string imageCaption = captionNode != null ? this.HtmlDecode(captionNode.InnerText) : string.Empty;
            string imageUrl = Utils.EnsureHttps(Utils.GetImageUrlFromImageTag(imgTag, this.currentUri.Host));
            var imageData = new ImagePathData(imageCaption, imageUrl);
            if (!this.foundFirstH2 || this.currentSection == null)
                this.leadSection.ImagePaths.Add(imageData);
            else
                this.currentSection.ImagePaths.Add(imageData);
        }
    }

    private void HandleGalleryNode(HtmlNode node)
    {
        var galleryItems = node.SelectNodes(".//*[contains(@class, 'gallerybox')]");
        if (galleryItems == null) return;
        foreach (var item in galleryItems)
        {
            var imgTag = item.SelectSingleNode(".//img");
            var captionNode = item.SelectSingleNode(".//*[contains(@class, 'gallerytext')]");
            if (imgTag != null)
            {
                string imageCaption = captionNode != null ? this.HtmlDecode(captionNode.InnerText) : string.Empty;
                string imageUrl = Utils.EnsureHttps(Utils.GetImageUrlFromImageTag(imgTag, this.currentUri.Host));
                var imageData = new ImagePathData(imageCaption, imageUrl);
                if (!this.foundFirstH2 || this.currentSection == null)
                    this.leadSection.ImagePaths.Add(imageData);
                else
                    this.currentSection.ImagePaths.Add(imageData);
            }
        }
    }
}