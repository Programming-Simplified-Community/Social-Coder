namespace SocialCoder.Web.Server.Services;

/// <summary>
/// Used to determine if the application is in setup mode
/// </summary>
public class AppStateProvider
{
    public bool IsInSetupMode { get; }

    public AppStateProvider(IConfiguration configuration)
    {
        this.IsInSetupMode = !configuration.GetValue("IsSetupComplete", false);
    }
}