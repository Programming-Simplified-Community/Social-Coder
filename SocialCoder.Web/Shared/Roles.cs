namespace SocialCoder.Web.Shared;

public static class Roles
{
    
    /// <summary>
    /// Highest level access + owner
    /// </summary>
    public const string Owner = "Owner";
    
    /// <summary>
    /// Highest level access (outside of being an owner)
    /// </summary>
    public const string Administrator = "Administrator";

    /// <summary>
    /// Access to event-based items
    /// </summary>
    public const string EventCoordinator = "Event-Coordinator";

    /// <summary>
    /// Comma separated string containing roles that are considered elevated
    /// </summary>
    public const string ELEVATED_ROLES = $"{Owner},{Administrator}";
}