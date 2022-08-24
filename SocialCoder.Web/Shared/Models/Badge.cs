namespace SocialCoder.Web.Shared.Models;

/// <summary>
/// Archetype for type of badge
/// </summary>
public enum BadgeType
{
    Badge,
    Quest
}

/// <summary>
/// Represents user-progress. Something to showoff.
/// </summary>
public class Badge
{
    public int Id { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }
    
    /// <summary>
    /// Image path from wwwroot to use
    /// </summary>
    public string ImagePath { get; set; }
    
    /// <summary>
    /// The amount of 'whatever' the user has to complete to acquire this badge
    /// </summary>
    public int Requirement { get; set; }
    
    /// <summary>
    /// Amount of experience to reward users upon completion
    /// </summary>
    public int RewardExperience { get; set; }
    
    /// <summary>
    /// Archetype of badge
    /// </summary>
    public BadgeType BadgeType { get; set; } = BadgeType.Badge;

    /// <summary>
    /// Navigational property in EF that will create a "shadow" table creating this relationship
    /// </summary>
    public List<BadgeRequirement> Requirements = new();
}