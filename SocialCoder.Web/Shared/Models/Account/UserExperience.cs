using SocialCoder.Web.Shared.Enums;

namespace SocialCoder.Web.Shared.Models.Account;

public class UserExperience
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to user
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Foreign key to <see cref="ExperienceLevel"/>
    /// </summary>
    public int ExperiencePoolId { get; set; }

    /// <summary>
    /// Amount of experience
    /// </summary>
    public ExperienceLevel Level { get; set; }
}