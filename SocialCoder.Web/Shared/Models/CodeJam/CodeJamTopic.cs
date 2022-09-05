namespace SocialCoder.Web.Shared.Models.CodeJam;

public class CodeJamTopic
{
    public int Id { get; set; }

    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;

    /// <summary>
    /// URL to image that will be used for a code jam
    /// </summary>
    public string? BackgroundImageUrl { get; set; }

    /// <summary>
    /// Is this topic available? Equivalent of a soft-delete
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// At this time applicants are no longer accepted. The topic is "active"
    /// </summary>
    public DateTime JamStartDate { get; set; }

    /// <summary>
    /// After this time, submissions are no longer accepted. The "jam" is considered closed.
    /// </summary>
    public DateTime JamEndDate { get; set; }

    /// <summary>
    /// Time in which this topic will start accepting applicants
    /// </summary>
    public DateTime RegistrationStartDate { get; set; }
    
}