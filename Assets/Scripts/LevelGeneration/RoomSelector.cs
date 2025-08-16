using System.Collections.Generic;

public class RoomAndRating
{
    public Room Room { get; set; }
    public RatingResult Rating { get; set; }
}

public class RoomSelector
{
    public RoomAndRating FindBestRoom(List<Room> rooms, LevelGenRequirements reqs)
    {
        Room bestRoom = null;
        RatingResult ratingResult = null;
        foreach (var room in rooms)
        {
            var rating = room.RateRequirementsMatch(reqs);
            if (!rating.IsValid)
            {
                continue;
            }

            if (ratingResult == null || rating.Score > ratingResult.Score)
            {
                bestRoom = room;
                ratingResult = rating;
            }
        }

        return new RoomAndRating
        {
            Room = bestRoom,
            Rating = ratingResult
        };
    }
}
