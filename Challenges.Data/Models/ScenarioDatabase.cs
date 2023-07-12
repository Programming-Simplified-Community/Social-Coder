namespace Challenges.Data.Models;

public class ScenarioDatabase
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to <see cref="Scenario"/>
    /// </summary>
    public int ScenarioId { get; set; }

    /// <summary>
    /// Navigational Property.
    /// </summary>
    public Scenario Scenario { get; set; }

    /// <summary>
    /// Image to utilize for the database
    /// </summary>
    public string DockerImage { get; set; }
    
    /// <summary>
    /// Pat hto use if we want to persist data
    /// </summary>
    private string? InsideContainerDatabaseLocation { get; set; }

    /// <summary>
    /// Health check to use, to help us determine if it's appropriate to start linked applications
    /// </summary>
    public string? HealthCheck { get; set; }

    /// <summary>
    /// Should the database be pre-configured prior to giving access to the user
    /// </summary>
    public bool UseCustomMigrations { get; set; }

    /// <summary>
    /// Path on disk where the database migrations should live
    /// </summary>
    public string? MigrationsScriptPath { get; set; }

    /// <summary>
    /// Command to execute to start/initiate the migration
    /// </summary>
    public string? MigrationsCommand { get; set; }
}