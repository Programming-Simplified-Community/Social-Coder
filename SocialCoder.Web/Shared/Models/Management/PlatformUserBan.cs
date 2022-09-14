using System.ComponentModel.DataAnnotations;
using SocialCoder.Web.Shared.Interfaces;

namespace SocialCoder.Web.Shared.Models.Management;

/// <summary>
/// Represents a platform wide ban
/// </summary>
public class PlatformUserBan : IAuditable
{
    public int Id { get; set; }

    /// <summary>
    /// Reasoning behind ban
    /// </summary>
    [MinLength(10)]
    public string Reason { get; set; }
    
    /// <summary>
    /// Foreign key to user who is being banned
    /// </summary>
    public string UserId { get; set; }

    public string CreatedByUserId { get; set; }
    public string? EditedByUserId { get; set; } = null;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? EditedOn { get; set; } = null;
}