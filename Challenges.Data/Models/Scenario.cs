namespace Challenges.Data.Models;

public class Scenario
{
    public int Id { get; set; }

    /// <summary>
    /// Generic title for this scenario
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Basic description of scenario, does not include acceptance criteria
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Requirements users must meet in order to receive points
    /// </summary>
    public string? AcceptanceCriteria { get; set; }

    /// <summary>
    /// Indicator for users to determine if this scenario is up their alley
    /// </summary>
    public Difficulty Difficulty { get; set; } = Difficulty.Beginner;

    /// <summary>
    /// Total number of points this scenario will provide if all tests pass successfully
    /// </summary>
    public int TotalPoints { get; set; }

    /// <summary>
    /// Can users receive partial credit if they don't pass 100% of the tests
    /// </summary>
    public bool AllOrNothing { get; set; }

    /// <summary>
    /// Optional amount of time in minutes. If a test takes more than specified amount of time, it will shut down.
    /// Otherwise, no timeout
    /// </summary>
    public int? TimeoutInMinutes { get; set; }
}