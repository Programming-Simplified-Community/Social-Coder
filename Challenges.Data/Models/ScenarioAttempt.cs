namespace Challenges.Data.Models;

public class ScenarioAttempt
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to User 
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// User public repository to grab submission from
    /// </summary>
    public string? GithubUrl { get; set; }

    /// <summary>
    /// Specific branch to checkout from repo
    /// </summary>
    public string? GithubBranch { get; set; }
    
    /// <summary>
    /// Encoded text of user submission. Alternative way of submitting
    /// </summary>
    public string? EncodedText { get; set; }

    /// <summary>
    /// Number of times user has tried to execute this solution
    /// </summary>
    public int NumberOfAttempts { get; set; }

    /// <summary>
    /// Total time it took to complete scenario if applicable
    /// </summary>
    public string? Duration { get; set; }

    /// <summary>
    /// Language the attempt is intended for
    /// </summary>
    public ProgrammingLanguage Language { get; set; }
}