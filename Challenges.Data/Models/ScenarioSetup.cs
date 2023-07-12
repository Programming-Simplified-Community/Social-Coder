namespace Challenges.Data.Models;

public class ScenarioSetup
{
    public int Id { get; set; }

    /// <summary>
    /// Language this setup information is intended for
    /// </summary>
    public ProgrammingLanguage Language { get; set; }

    /// <summary>
    /// Version of docker image to grab (otherwise latest)
    /// </summary>
    public string? LanguageVersion { get; set; }

    /// <summary>
    /// Docker image to utilzie when building container
    /// </summary>
    public string DockerImage { get; set; }

    /// <summary>
    /// Docker entrypoint to utilize when starting container
    /// </summary>
    public string DockerEntrypoint { get; set; }
    
    /// <summary>
    /// Path inside the container where files should be located to execute 
    /// </summary>
    public string? InsideContainerMountLocation { get; set; }

    /// <summary>
    /// Foreign key to <see cref="ScenarioDatabase"/>
    /// </summary>
    public int ScenarioDatabaseId { get; set; }
    
    /// <summary>
    /// Navigational Property
    /// </summary>
    public ScenarioDatabase ScenarioDatabase { get; set; }
}