namespace SocialCoder.Web.Shared.Requests.Management.Users;

public record RemoveRoleFromUserRequest(string UserId, string RoleName, string? Reason);
