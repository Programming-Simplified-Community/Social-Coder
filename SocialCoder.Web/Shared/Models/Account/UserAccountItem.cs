namespace SocialCoder.Web.Shared.Models.Account;

public class UserAccountItem
{
    public string UserId { get; init; }
    public string Username { get; init; }
    public string Email { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}