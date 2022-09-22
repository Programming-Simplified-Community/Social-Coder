namespace SocialCoder.Web.Shared.Interfaces;

/// <summary>
/// Basic set of rules/behaviors for auditable entities
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// Foreign key to user who created this record
    /// </summary>
    string CreatedByUserId { get; }

    /// <summary>
    /// Foreign key to user who edited this record
    /// </summary>
    string? EditedByUserId { get; }

    /// <summary>
    /// Date in which this record was created
    /// </summary>
    DateTime CreatedOn { get; }
    
    /// <summary>
    /// Date in which this record was last modified (excluding creation)
    /// </summary>
    DateTime? EditedOn { get; }
}