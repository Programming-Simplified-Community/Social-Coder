namespace SocialCoder.Web.Shared.Requests.Management.Users;

public class EditProfileInfoRequest
{
    public string UserId { get; set; }
    public string? DisplayName { get; set; }
    public string? Language { get; set; }
    public string? Country { get; set; }
}