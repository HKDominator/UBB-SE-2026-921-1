namespace PussyCats.Library.Domain;

public class Recommendation
{
    public int RecommendationId { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int JobId { get; set; }
    public Job Job { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}
