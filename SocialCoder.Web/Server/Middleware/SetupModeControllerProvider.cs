using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using SocialCoder.Web.Server.Attributes;

namespace SocialCoder.Web.Server.Middleware;

/// <summary>
/// This class is used to remove controllers from the application when the application is in setup mode.
/// </summary>
public class SetupModeControllerProvider : IApplicationFeatureProvider<ControllerFeature>
{
    private readonly bool _isInSetupMode;

    public SetupModeControllerProvider(bool isInSetupMode)
    {
        this._isInSetupMode = isInSetupMode;
    }

    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        var controllerTypesToRemove = !this._isInSetupMode
            ? feature.Controllers
                .Where(x => x.GetCustomAttributes<OnlyInSetupModeAttribute>().Any())
                .ToList()
            : feature.Controllers
                .Where(x => x.GetCustomAttributes<DisabledInSetupModeAttribute>().Any())
                .ToList();

        foreach (var controller in controllerTypesToRemove)
        {
            feature.Controllers.Remove(controller);
        }
    }
}