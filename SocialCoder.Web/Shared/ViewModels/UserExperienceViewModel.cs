using SocialCoder.Web.Shared.Enums;

namespace SocialCoder.Web.Shared.ViewModels;

public class UserExperienceViewModel
{
    public string UserId { get; set; }
    public int ExperiencePoolId { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public ExperienceLevel Experience { get; set; }
}