using SocialCoder.Web.Shared.Models.Setup;

namespace SocialCoder.Web.Server.Models;

public class AppSettings
{
    public bool IsSetupComplete { get; set; }

    public PostgresConnection? Postgres { get; set; }
    public Dictionary<string, OAuthSetting> OAuthSettings { get; init; } = [];
}