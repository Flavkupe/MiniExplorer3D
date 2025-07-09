using System;
using System.Collections.Generic;
using System.Linq;

public static class RatingProcessor
{
    public static float CalculateRating(RoomData roomData)
    {
        if (roomData == null)
        {
            throw new ArgumentNullException(nameof(roomData), "Room data cannot be null.");
        }
        // Example rating calculation based on room dimensions and number of exhibits
        float baseRating = roomData.DimX * roomData.DimY * roomData.DimZ;
        float exhibitBonus = roomData.ExhibitData.Count * 10f; // Each exhibit adds 10 points
        return baseRating + exhibitBonus;
    }


    public static RatingResult RateExhibitMatch(Exhibit exhibit, SectionData section)
    {
        if (section == null)
        {
            return RatingResult.NoMatch;
        }

        if (section == null || !exhibit.CanHandleSection(section))
        {
            DebugLogger.Log($"----Exhibit [{section.Title}] [{exhibit.PrefabID}]: No Match", LoggerFilter.LogRatings);
            return RatingResult.NoMatch;
        }

        float score = 5f;

        // Title/AreaTitleSign
        bool hasTitle = !string.IsNullOrEmpty(section.Title);
        if (hasTitle)
        {
            score += exhibit.SupportsTitle() ? 1f : -1f;
        }

        // Reading placeholders vs LocationText
        int textCount = section.LocationText?.Count ?? 0;
        int readingCount = exhibit.GetReadingCount();
        score += ScoreCountMatch(readingCount, textCount, 1f);

        int imagePodiumCount = exhibit.DisplayPodiums.Count(a => a.CanHandlePodiumImage);
        score += ScoreCountMatch(exhibit.Paintings.Length, section.ImagePaths.Count, 2f);
        score += ScoreCountMatch(imagePodiumCount, section.PodiumImages.Count, 2f);
        score += ScoreCountMatch(exhibit.Exits.Count, section.Exits.Count, 2f);

        // Subsections and subexhibits
        score += ScoreSubsections(exhibit, section);

        DebugLogger.Log($"----Exhibit [{section.Title}] [{exhibit.PrefabID}]: {score}", LoggerFilter.LogRatings);

        return new RatingResult(score, true);
    }

    public static RatingResult RateRoomMatch(Room room, LevelGenRequirements reqs)
    {
        if (reqs == null || reqs.SectionData == null || reqs.SectionData.Count == 0)
        {
            return RatingResult.NoMatch;
        }

        return GetRoundRobinScore(room.Exhibits, reqs.SectionData);
    }

    /// <summary>
    /// Helper to score how well two counts match. Prefers exact match, then too many, then too few. The more difference, the worse the score.
    /// </summary>
    private static float ScoreCountMatch(int exhibitCount, int requiredCount, float weight)
    {
        if (requiredCount == 0)
        {
            return 0f;
        }

        if (requiredCount > 0 && exhibitCount == 0)
        {
            // If required count is > 0 but we have none, this is a bad match
            return -weight;
        }

        int diff = Math.Abs(exhibitCount - requiredCount);
        if (diff == 0)
        {
            return weight; // perfect match
        }
        else if (diff > 0)
        {
            // Too many: small penalty per extra
            return weight - (0.25f * diff * weight);
        }
        else
        {
            // Too few: larger penalty per missing
            return weight - (0.5f * diff * weight);
        }
    }

    private static float ScoreSubsections(Exhibit exhibit, SectionData section)
    {
        var fullScore = GetRoundRobinScore(exhibit.SubExhibits, section.Subsections);
        return fullScore.Score;
    }

    /// <summary>
    /// Sequentially rates sections against available exhibits in a round-robin fashion.
    /// </summary>
    private static RatingResult GetRoundRobinScore(IList<ExhibitBase> exhibits, IList<SectionData> sections)
    {
        if (exhibits == null || sections == null || exhibits.Count == 0 || sections.Count == 0)
        {
            return RatingResult.NoMatch;
        }

        var sectionsToCheck = new Queue<SectionData>(sections);
        var exhibitsAvailable = new HashSet<ExhibitBase>(exhibits);
        var matchedSections = new List<RatingResultMatch>();
        var score = 0.0f;
        while (sectionsToCheck.Count > 0)
        {
            if (exhibitsAvailable.Count == 0)
            {
                // No more exhibits to match, break out
                break;
            }

            var section = sectionsToCheck.Dequeue();

            var bestMatch = GetBestMatchRating(exhibitsAvailable, section);
            if (!bestMatch.IsValid || bestMatch.Match == null)
            {
                // skip this section (for now)
                continue;
            }

            matchedSections.Add(new RatingResultMatch(section, bestMatch.Match.PrefabID));
            score += bestMatch.Score;
            exhibitsAvailable.Remove(bestMatch.Match);
        }

        if (matchedSections.Count == 0)
        {
            return RatingResult.NoMatch;
        }

        score -= exhibitsAvailable.Count * 0.5f; // penalize for unused exhibits

        return new RatingResult(score, true)
        {
            MatchedSections = matchedSections
        };
    }

    private static RatingResult<ExhibitBase> GetBestMatchRating(IEnumerable<ExhibitBase> exhibits, SectionData section)
    {
        float bestScore = float.MinValue;
        int matchCount = 0;
        ExhibitBase bestMatch = null;
        foreach (var exhibit in exhibits)
        {
            var result = exhibit.RateSectionMatch(section);
            if (!result.IsValid)
            {
                continue;
            }
            matchCount++;
            if (result.Score > bestScore)
            {
                bestScore = result.Score;
                bestMatch = exhibit;
            }
        }
        if (matchCount == 0)
        {
            return RatingResult<ExhibitBase>.NoMatch;
        }

        return new RatingResult<ExhibitBase>(bestScore, bestMatch);
    }
}