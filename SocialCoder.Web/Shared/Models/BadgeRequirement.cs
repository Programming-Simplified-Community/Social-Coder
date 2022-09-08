#pragma warning disable CS8618
namespace SocialCoder.Web.Shared.Models;

public class BadgeRequirement
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to badge we're adding requirements to
    /// </summary>
    public int BadgeId { get; set; }
    
    /// <summary>
    /// Foreign key to Badge required to be completed in order for <see cref="BadgeId"/> to be unlocked
    /// </summary>
    public int RequiredBadgeId { get; set; }

    public Badge Badge { get; set; }
}