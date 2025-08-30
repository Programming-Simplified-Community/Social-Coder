using System.ComponentModel.DataAnnotations;
using System.Reflection;
using SocialCoder.Web.Shared.Enums;

namespace SocialCoder.Web.Shared.Extensions;

public static class ExperienceItemExtensions
{
    private static readonly Dictionary<ExperienceItem, string> DisplayNameCache = new();

    public static string GetDisplayName(this ExperienceItem item)
    {
        if (DisplayNameCache.TryGetValue(item, out var value))
        {
            return value;
        }

        var type = item.GetType();
        var memberInfo = type.GetMember(item.ToString());
        var attributes = memberInfo.First().GetCustomAttributes<DisplayAttribute>(false).ToList();

        var name = Enum.GetName(typeof(ExperienceItem), item) ?? "Unknown";

        if (!attributes.Any())
        {
            DisplayNameCache.Add(item, name);
            return name;
        }

        name = attributes.First().Name ?? "Unknown";
        DisplayNameCache.Add(item, name);
        return name;
    }
}