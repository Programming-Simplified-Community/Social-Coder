namespace SocialCoder.Web.Shared.Models.Account;

public class UserAccountItem
{
    public string UserId { get; init; }
    public string Username { get; init; }
    public string Email { get; set; }
    public bool IsBanned { get; set; }
    public string? BanReason { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}

public record UserBannedInfo(string UserId, bool IsBanned, string? BanReason);
public record USerDeletedInfo(string UserId, string Username);