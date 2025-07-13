using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class WikipediaGenerator : WebLevelGenerator
{
    private WikipediaArticleProcessor articleProcessor = new();
    private WikipediaMainPageProcessor mainPageProcessor = new();

    private const string RandomArtileAPICall = "https://en.wikipedia.org/w/api.php?action=query&list=random&rnnamespace=0&rnlimit=1&format=json";

    private const string MainPageName = "Main_Page";

    public override IEnumerator GenerateRandom(MonoBehaviour caller)
    {
        var requestor = new WebRequestor();
        yield return requestor.MakeWebRequest(RandomArtileAPICall, true);
        if (!string.IsNullOrEmpty(requestor.Error))
        {
            Debug.LogWarning($"Failed to fetch random article, loading Main Page instead: {requestor.Error}");
            LoadMainPage();
            yield break;
        }

        if (string.IsNullOrEmpty(requestor.JsonResult))
        {
            Debug.LogWarning("No data received from Wikipedia API for random article.");
            LoadMainPage();
            yield break;
        }

        try
        {
            var obj = JObject.Parse(requestor.JsonResult);
            var randomArticle = obj["query"]?["random"]?.FirstOrDefault();
            if (randomArticle != null)
            {
                string title = randomArticle["title"]?.ToString();
                if (!string.IsNullOrEmpty(title))
                {
                    SceneLoader.Instance.LoadWikipediaArticle(title);
                    yield break;
                }
                else
                {
                    Debug.LogWarning("Random article title is empty, loading Main Page instead.");
                    LoadMainPage();
                    yield break;
                }
            }
            else
            {
                Debug.LogWarning("No random article found, loading Main Page instead.");
                LoadMainPage();
                yield break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing random article response: {ex.Message}");
            LoadMainPage();
            yield break;
        }
    }

    private void LoadMainPage()
    {
        SceneLoader.Instance.LoadWikipediaArticle(MainPageName);
    }

    public override IEnumerator PrepareAreaGeneration(Location location, MonoBehaviour caller)
    {
        if (!location.NeedsInitialization)
        {
            this.CallOnAreaGenReady(new AreaGenerationReadyEventArgs() { AreaLocation = StageManager.CurrentLocation });
            yield break;
        }

        // Extract the Wikipedia page title from the URL
        string pageTitle = null;
        try
        {
            var uri = new Uri(location.Path);
            // Wikipedia URLs are like https://en.wikipedia.org/wiki/Page_Title
            var segments = uri.Segments;
            if (segments.Length > 0)
            {
                pageTitle = segments.Last().TrimEnd('/');
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to parse Wikipedia URL: {location.Path} ({ex.Message})");
        }

        if (string.IsNullOrEmpty(pageTitle))
        {
            Debug.LogWarning($"Could not determine Wikipedia page title from URL: {location.Path}");
            location.LocationData.RawData = string.Empty;
        }
        else
        {
            var page = UnityWebRequest.EscapeURL(pageTitle);
            string apiUrl = $"https://en.wikipedia.org/w/api.php?action=parse&page={page}&format=json&origin=*";

            var requestor = new WebRequestor();
            yield return requestor.MakeWebRequest(apiUrl);

            if (!string.IsNullOrEmpty(requestor.Error) || string.IsNullOrEmpty(requestor.JsonResult))
            {
                Debug.LogWarning($"No data received from Wikipedia API for {pageTitle}");
                location.LocationData.RawData = string.Empty;
                yield break;
            }

            string json = requestor.JsonResult;
            string html = ExtractHtmlFromWikipediaApiJson(json);
            string title = ExtractTitleFromWikipediaApiJson(json);
            location.LocationData.RawData = html;
            location.Name = title;
        }


        this.CallOnAreaGenReady(new AreaGenerationReadyEventArgs() { AreaLocation = StageManager.CurrentLocation });
        yield return null;
    }


    // Helper to extract the HTML from the Wikipedia API JSON response
    private string ExtractHtmlFromWikipediaApiJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return string.Empty;
        try
        {
            var obj = JObject.Parse(json);
            var html = obj["parse"]?["text"]?["*"]?.ToString();
            return html ?? string.Empty;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to parse Wikipedia API JSON: {ex.Message}");
            return string.Empty;
        }
    }

    // Helper to extract the title from the Wikipedia API JSON response
    protected string ExtractTitleFromWikipediaApiJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return string.Empty;
        try
        {
            var obj = JObject.Parse(json);
            var title = obj["parse"]?["title"]?.ToString();
            return title ?? string.Empty;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to parse Wikipedia API JSON for title: {ex.Message}");
            return string.Empty;
        }
    }

    protected override Room GetFirstRoom(Location location)
    {
        Room room = null;
        if (LocationIsMainPage(location))
        {
            room = StageManager.SceneLoader.EntranceRoomPrefabs.GetRandom();
        }
        else
        {
            room = StageManager.SceneLoader.StartingRoomPrefabs.GetRandom();
        }

        room.PopulateParts();
        return room;
    }

    protected override void ProcessHtmlDocument(MainLocation location, Uri currentUri)
    {
        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(location.LocationData.RawData);

        if (LocationIsMainPage(location))
        {
            mainPageProcessor.ProcessHtml(location, htmlDoc, currentUri);
        }
        else
        {
            articleProcessor.ProcessHtml(location, htmlDoc, currentUri);
        }
    }

    private bool LocationIsMainPage(Location location)
    {
        return location.Path.EndsWith(MainPageName);
    }
}
