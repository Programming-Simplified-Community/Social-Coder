namespace Challenges.Data.Models;

public class ScenarioHint
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to <see cref="Scenario"/>
    /// </summary>
    public int ScenarioId { get; set; }
    
    /// <summary>
    /// Navigational property
    /// </summary>
    public Scenario Scenario { get; set; }

    /// <summary>
    /// Title text for hint
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The main content for what shall appear to end user
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Language this hint is for
    /// </summary>
    public ProgrammingLanguage Language { get; set; }
}