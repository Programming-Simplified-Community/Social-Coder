namespace SocialCoder.Web.Shared.ViewModels;

public class LoginResult
{
    public string Message { get; init; }
    public string Email { get; init; }
    public string Username { get; init; }
    public string JwtBearer { get; init; }
    public bool Success { get; init; }
}