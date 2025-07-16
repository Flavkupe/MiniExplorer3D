
public abstract class LoggingDataObject
{
    public abstract string DataType { get; }
}

public class LoggingRoomRatingData : LoggingDataObject
{
    public override string DataType => "RoomRatingData";

    public string RoomPrefabID { get; set; }

    public float Score { get; set; }

    public float UnmachedExhibitPercentage { get; set; }
}


public class LoggingRatingData : LoggingDataObject
{
    public override string DataType => "RatingData";

    public string ExhibitPrefabID { get; set; }

    public string ExhibitParentPrefabID { get; set; }

    public float Score { get; set; }

    public int SectionReadingCount { get; set; }

    public int SectionImageCount { get; set; }

    public int SectionListCount { get; set; }

    public bool SectionHasTitle { get; set; }

    public string SectionName { get; set; }

    public string SectionParentName { get; set; }

    public string SectionType { get; set; }

    public float ReadingScore { get; set; }
    public float ImageScore { get; set; }
    public float ExitsScore { get; set; }
    public float SubsectionScore { get; set; }
    public float TitleScore { get; set; }

    public LoggingRatingData(ExhibitBase exhibit, SectionData section, RatingResult ratingResult)
    {
        ExhibitPrefabID = exhibit.PrefabID;
        ExhibitParentPrefabID = exhibit.transform.parent != null
            ? exhibit.transform.parent.GetComponent<IMatchesPrefab>()?.PrefabID
            : null;

        Score = ratingResult.Score;
        SectionType = section.SectionType.ToString();
        SectionReadingCount = section.LocationText.Count;
        SectionImageCount = section.ImagePaths.Count;
        SectionListCount = section.Lists.Count;
        SectionHasTitle = !string.IsNullOrEmpty(section.Title);
        SectionName = section.Title;
        SectionParentName = section.ParentTitle;
        ReadingScore = ratingResult.ReadingScore;
        ImageScore = ratingResult.ImageScore;
        ExitsScore = ratingResult.ExitsScore;
        SubsectionScore = ratingResult.SubsectionScore;
        TitleScore = ratingResult.TitleScore;
    }
}
