using System;
using System.Collections.Generic;

public static class RatingProcessor
{
    public static RatingResult RateExhibitMatch(Exhibit exhibit, SectionData section)
    {
        if (section == null)
        {
            return RatingResult.NoMatch;
        }

        if (!exhibit.CanHandleSection(section))
        {
            DebugLogger.Log($"----Exhibit [{section.Title}] [{exhibit.PrefabID}]: No Match", LoggerFilter.LogRatings);
            return RatingResult.NoMatch;
        }

        float score = 50f;

        float baseWeight = 10.0f;

        // Title/AreaTitleSign
        bool hasTitle = !string.IsNullOrEmpty(section.Title);
        var titleScore = 0.0f;
        if (hasTitle)
        {
            titleScore += exhibit.SupportsTitle ? baseWeight : -baseWeight;
        }

        // Reading placeholders vs LocationText
        int textCount = section.LocationText?.Count ?? 0;

        // TODO: handle reading which uses multiple list items
        textCount += section.Lists.Count;

        int readingCount = exhibit.GetReadingCount();
        var readingScore = ScoreCountMatch(readingCount, textCount, baseWeight);

        int imageCount = exhibit.GetPaintingCount();
        var imageScore = ScoreCountMatch(imageCount, section.ImagePaths.Count, baseWeight * 2f);
        var exitsScore = ScoreCountMatch(exhibit.Exits.Count, section.Exits.Count, baseWeight * 2f);

        // Subsections and subexhibits
        var subsectionScore = ScoreSubsections(exhibit, section);

        score += titleScore + readingScore + imageScore + exitsScore + subsectionScore;
        var result = new RatingResult(score, true);
        result.SubsectionScore = subsectionScore;
        result.ReadingScore = readingScore;
        result.ImageScore = imageScore;
        result.ExitsScore = exitsScore;
        result.TitleScore = titleScore;

        DebugLogger.LogSample(new LoggingRatingData(exhibit, section, result));

        return result;
    }

    public static RatingResult RateRoomMatch(Room room, LevelGenRequirements reqs)
    {
        if (reqs == null || reqs.SectionData == null || reqs.SectionData.Count == 0)
        {
            return RatingResult.NoMatch;
        }

        var result = GetRoundRobinScore(room.Exhibits, reqs.SectionData);
        DebugLogger.LogSample(new LoggingRoomRatingData
        {
            RoomPrefabID = room.Name,
            Score = result.Score,
            UnmachedExhibitPercentage = result.UnusedPercentage,
        });

        return result;
    }

    /// <summary>
    /// Helper to score how well two counts match. Prefers exact match, then too many, then too few. The more difference, the worse the score.
    /// </summary>
    private static float ScoreCountMatch(int exhibitCount, int requiredCount, float weight)
    {
        // score not applicable
        if (exhibitCount == 0 && requiredCount == 0)
        {
            return 0f;
        }

        int diff = Math.Abs(exhibitCount - requiredCount);

        // If required count is > 0 but we have none, this is a bad match
        if (exhibitCount == 0 && requiredCount > 0)
        {
            return -weight * diff;
        }

        if (diff == 0)
        {
            return weight; // perfect match
        }

        if (requiredCount == 0)
        {
            // If we have exhibits but no required count, this is a bad match;
            // take a small penalty for each extra exhibit, as other parts of the
            // exhibit may still match well.
            return -(0.2f * diff * weight);
        }

        // these are partial matches, so the score should be positive but reduced
        if (exhibitCount > requiredCount)
        {
            // Too many exhibits: small penalty per unused exhibit
            return Math.Max(weight - (0.2f * diff), 0.1f);
        }
        else
        {
            // Too few exhibits: larger penalty per missing exhibit
            return Math.Max(weight - (0.5f * diff), 0.1f);
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

        var unmatchedPercentage = (float)exhibitsAvailable.Count / (float)exhibits.Count;

        // penalize for unused exhibits
        score *= 1.0f - unmatchedPercentage;

        return new RatingResult(score, true)
        {
            MatchedSections = matchedSections,
            UnusedPercentage = unmatchedPercentage,
        };
    }

    private static RatingResult<ExhibitBase> GetBestMatchRating(IEnumerable<ExhibitBase> exhibits, SectionData section)
    {
        float bestScore = float.MinValue;
        int matchCount = 0;
        ExhibitBase bestMatch = null;

        // shuffling the list will allow ties to be randomized for better variety
        var exhibitList = new List<ExhibitBase>(exhibits).Shuffle();
        foreach (var exhibit in exhibitList)
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