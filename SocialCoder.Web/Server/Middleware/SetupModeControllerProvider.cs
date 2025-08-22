using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using SocialCoder.Web.Server.Attributes;

namespace SocialCoder.Web.Server.Middleware;

public class SetupModeControllerProvider : IApplicationFeatureProvider<ControllerFeature>
{
    private readonly bool _isInSetupMode;

    public SetupModeControllerProvider(bool isInSetupMode)
    {
        this._isInSetupMode = isInSetupMode;
    }

    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        if (!this._isInSetupMode)
        {
            return;
        }

        var controllersToMove = feature.Controllers
            .Where(x => x.GetCustomAttributes<DisabledInSetupModeAttribute>().Any())
            .ToList();

        foreach (var controller in controllersToMove)
        {
            feature.Controllers.Remove(controller);
        }
    }
}