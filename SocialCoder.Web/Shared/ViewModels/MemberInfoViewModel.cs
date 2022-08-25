namespace SocialCoder.Web.Shared.ViewModels;

public class MemberInfoViewModel
{
    /// <summary>
    /// Unique User Id
    /// </summary>
    public string UserId { get; set; }
    
    /// <summary>
    /// Primary display name for user on the website
    /// </summary>
    public string Username { get; set; }
    
    /// <summary>
    /// Name user also goes by
    /// </summary>
    public string AlternateName { get; set; }
    
    /// <summary>
    /// Avatar image for user
    /// </summary>
    public string UserImage { get; set; }
    
    /// <summary>
    /// Current user level
    /// </summary>
    public int UserLevel { get; set; }

    /// <summary>
    /// Number of posts this user has made
    /// </summary>
    public int NumberOfPosts { get; set; }
    
    /// <summary>
    /// Number of friends this user has
    /// </summary>
    public int NumberOfFriends { get; set; }

    /// <summary>
    /// Any social media links this user may have on their profile
    /// </summary>
    public List<UserSocialMediaItem> Socials { get; set; } = new();
    
    /// <summary>
    /// Any badges this user has
    /// </summary>
    public List<BasicBadgeViewModel> Badges { get; set; } = new();
}