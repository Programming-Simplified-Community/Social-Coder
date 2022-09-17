using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using SocialCoder.Web.Client.Services.Implementations;

namespace SocialCoder.Web.Client.Components;

public class AuthenticatedComponent : ComponentBase
{
    [Inject] protected IdentityAuthenticationStateProvider AuthProvider { get; set; }

    protected ClaimsPrincipal User { get; private set; }
    protected string Name { get; private set; }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        var auth = await AuthProvider.GetAuthenticationStateAsync();

        User = auth.User;
        Name = auth.User.Identity?.Name ?? string.Empty;
    }
}