namespace SocialCoder.Web.Shared.ViewModels;

public class UserInfo
{
    public bool IsAuthenticated { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public Dictionary<string, string> Claims { get; set; }
}