using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WikipediaArticleProcessor : WikipediaBaseProcessor
{
    // Private fields for shared state
    private SectionData leadSection;
    private SectionData currentSection;
    private SectionData parentSection;
    private bool foundFirstH2;
    private Uri currentUri;

    private static readonly string[] SkippedSectionTitles = new[] {
        "references", "external links", "bibliography", "further reading", "notes", "see also"
    };

    private bool IsSkippedSectionTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return false;
        string lower = title.Trim().ToLowerInvariant();
        return SkippedSectionTitles.Any(skip => lower == skip);
    }

    public override void ProcessHtml(MainLocation location, HtmlDocument htmlDoc, Uri currentUri)
    {
        this.currentUri = currentUri;
        this.leadSection = new SectionData { SectionType = SectionType.Main };
        this.currentSection = null;
        this.foundFirstH2 = false;

        this.leadSection.Title = location.Name;
        if (StageManager.CurrentLocation.Path == location.Path)
        {
            StageManager.CurrentLocation.Name = location.Name;
        }

        HtmlNode contentNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'mw-parser-output')]");
        if (contentNode == null)
        {
            Debug.LogWarning("Content node not found in Wikipedia page.");
            return;
        }

        location.LocationData.Sections.Clear();

        var infoboxNode = contentNode.SelectSingleNode(".//table[contains(@class, 'infobox')]");
        if (infoboxNode != null)
        {
            var infoboxSection = ParseInfoboxTable(infoboxNode, location.Name);
            if (infoboxSection != null)
            {
                location.LocationData.Sections.Add(infoboxSection);
            }
        }

        foreach (var node in contentNode.ChildNodes)
        {
            if (node.Name == "h2")
            {
                this.HandleH2Node(node, location);
            }
            else if (IsNodeH2Wrapper(node))
            {
                var h2 = node.SelectSingleNode("h2");
                if (h2 != null)
                {
                    this.HandleH2Node(h2, location);
                    continue;
                }
            }

            else if (IsNodeH3Wrapper(node))
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
            else if (node.Name == "div" && NodeHasClass(node, "thumbinner"))
            {
                this.HandleThumbinnerDivNode(node);
            }
            else if (node.Name == "figure")
            {
                this.HandleFigureNode(node);
            }
            else if ((node.Name == "div" && NodeHasClass(node, "gallery")) ||
                        (node.Name == "ul" && NodeHasClass(node, "gallery")))
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
        // Skip unwanted sections
        if (IsSkippedSectionTitle(title)) {
            this.currentSection = null;
            this.parentSection = null;
            return;
        }

        this.parentSection = new SectionData
        {
            Title = title,
            Anchor = anchor,
            SectionType = SectionType.Standard
        };

        // empty section directly under h2, before the first h3
        this.currentSection = new SectionData
        {
            Title = string.Empty,
            Anchor = string.Empty,
            SectionType = SectionType.Subsection,
        };

        this.parentSection.Subsections.Add(this.currentSection);
        location.LocationData.Sections.Add(this.parentSection);
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
        ExtractLinks(node, textData, (IsInLeadSectionNode()) ? this.leadSection : this.currentSection, this.currentUri.Host);
        if (IsInLeadSectionNode())
        {
            this.leadSection.LocationText.Add(textData);
        }
        else
        {
            this.currentSection.LocationText.Add(textData);
        }
    }

    private void HandleListNode(HtmlNode node)
    {
        if (IsInLeadSectionNode())
        {
            HandleListNode(node, this.leadSection, this.currentUri.Host);
        }
        else
        {
            HandleListNode(node, this.currentSection, this.currentUri.Host);
        }
    }

    private void HandleTableNode()
    {
        // TODO
    }

    private bool IsInLeadSectionNode()
    {
        return !this.foundFirstH2 || this.currentSection == null;
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
            if (IsInLeadSectionNode())
            {
                this.leadSection.ImagePaths.Add(imageData);
            }
            else
            {
                this.currentSection.ImagePaths.Add(imageData);
            }
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
            if (IsInLeadSectionNode())
            {
                this.leadSection.ImagePaths.Add(imageData);
            }
            else
            {
                this.currentSection.ImagePaths.Add(imageData);
            }
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
                if (IsInLeadSectionNode())
                {
                    this.leadSection.ImagePaths.Add(imageData);
                }
                else
                {
                    this.currentSection.ImagePaths.Add(imageData);
                }
            }
        }
    }

    private SectionData ParseInfoboxTable(HtmlNode infoboxNode, string locationName)
    {
        var infoboxSection = new SectionData
        {
            Title = locationName,
            SectionType = SectionType.Infobox | SectionType.Main
        };

        var rows = infoboxNode.SelectNodes(".//tr");
        if (rows == null) return null;

        SectionData currentSubsection = null;
        var listItems = new List<ListItemsData>();
        foreach (var row in rows)
        {
            var th = row.SelectSingleNode("th");
            var td = row.SelectSingleNode("td");
            var tds = row.SelectNodes("td");
            bool isHeaderRow = th != null && td == null;
            bool isDataRow = tds != null && tds.Count > 0;

            if (isHeaderRow)
            {
                // Start a new subsection for this header
                currentSubsection = new SectionData
                {
                    Title = this.HtmlDecode(th.InnerText),
                    SectionType = SectionType.Subsection
                };
                infoboxSection.Subsections.Add(currentSubsection);
            }
            else if (isDataRow)
            {
                // If no header yet, create a default subsection
                if (currentSubsection == null)
                {
                    currentSubsection = new SectionData
                    {
                        Title = string.Empty,
                        SectionType = SectionType.Subsection
                    };
                    infoboxSection.Subsections.Add(currentSubsection);
                }

                // If this row has a th and tds, treat th as label
                string label = th != null ? this.HtmlDecode(th.InnerText) : null;
                string value = string.Empty;
                if (tds.Count > 1)
                {
                    value = string.Join(" ", tds.Cast<HtmlNode>().Select(cell => this.HtmlDecode(cell.InnerText)).Where(s => !string.IsNullOrWhiteSpace(s)));
                }
                else
                {
                    value = this.HtmlDecode(tds[0].InnerText);
                }


                // Add as text or list
                LocationTextData textData = null;
                if (!string.IsNullOrWhiteSpace(label))
                {
                    textData = new LocationTextData($"{label}: {value}");
                }
                else if (!string.IsNullOrWhiteSpace(value))
                {
                    textData = new LocationTextData(value);

                }

                // Add textData, if any, as a list item
                if (textData != null)
                {
                    if (currentSubsection.Lists.Count == 0)
                    {
                        currentSubsection.Lists.Add(new ListItemsData(new List<LocationTextData>()));
                    }
                    currentSubsection.Lists[0].Items.Add(textData);
                }

                // Handle all images in all tds
                foreach (var cell in tds.Cast<HtmlNode>())
                {
                    var imgTags = cell.SelectNodes(".//img");
                    if (imgTags != null)
                    {
                        foreach (var imgTag in imgTags)
                        {
                            string imageUrl = Utils.EnsureHttps(Utils.GetImageUrlFromImageTag(imgTag, this.currentUri.Host));
                            currentSubsection.ImagePaths.Add(new ImagePathData(string.Empty, imageUrl));
                        }
                    }

                    // Handle lists in td
                    var listNode = cell.SelectSingleNode("ul") ?? cell.SelectSingleNode("ol");
                    if (listNode != null)
                    {
                        HandleListNode(listNode, currentSubsection, this.currentUri.Host);
                    }
                }
            }
        }

        return infoboxSection;
    }

    private bool IsNodeH3Wrapper(HtmlNode node)
    {
        return node.Name == "div" && NodeHasClass(node, "mw-heading3");
    }

    private bool IsNodeH2Wrapper(HtmlNode node)
    {
        return node.Name == "div" && NodeHasClass(node, "mw-heading2");
    }

    private bool NodeHasClass(HtmlNode node, string className)
    {
        return node.GetAttributeValue("class", "").Contains(className);
    }
}