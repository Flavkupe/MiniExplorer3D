using HtmlAgilityPack;
using System;

public class WikipediaGenerator : WebLevelGenerator
{
    private WikipediaArticleProcessor articleProcessor = new();
    private WikipediaMainPageProcessor mainPageProcessor = new();

    private const string MainPageName = "Main_Page";

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
