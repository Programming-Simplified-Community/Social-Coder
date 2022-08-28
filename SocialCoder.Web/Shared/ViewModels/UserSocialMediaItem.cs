using SocialCoder.Web.Shared.Enums;

namespace SocialCoder.Web.Shared.ViewModels;

public class UserSocialMediaItem
{
    public string Url { get; set; } = string.Empty;
    public SocialMediaType Type { get; set; } = SocialMediaType.Discord;
}