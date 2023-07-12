namespace Challenges.Data.Models;

public class ScenarioReport
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to user
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Language this report is for
    /// </summary>
    public ProgrammingLanguage Language { get; set; }

    /// <summary>
    /// Time in which this report was created
    /// </summary>
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Total number of points the user has received
    /// </summary>
    public int PointsAwarded { get; set; }

    /// <summary>
    /// Total time it took to execute these tests
    /// </summary>
    public string? Duration { get; set; }

    /// <summary>
    /// Navigational Property for test results
    /// </summary>
    public List<ScenarioTestResult> ScenarioTestResults { get; set; } = new();
}