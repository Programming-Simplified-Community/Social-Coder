namespace SocialCoder.Web.Server.Services;

public class AppStateProvider
{
    public bool IsInSetupMode { get; }

    public AppStateProvider(IConfiguration configuration)
    {
        this.IsInSetupMode = !configuration.GetValue("IsSetupComplete", false);
    }
}