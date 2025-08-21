using Microsoft.Extensions.Options;
using SocialCoder.Web.Server.Models;

namespace SocialCoder.Web.Server.Services;

public class AppStateProvider
{
    private readonly IOptionsMonitor<AppSettings> _settings;

    public AppStateProvider(IOptionsMonitor<AppSettings> settings)
    {
        _settings = settings;
    }
    
    public bool IsInSetupMode => !_settings.CurrentValue.IsSetupComplete;
}