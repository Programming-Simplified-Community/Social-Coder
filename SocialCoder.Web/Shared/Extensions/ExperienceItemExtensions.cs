using System.ComponentModel.DataAnnotations;
using System.Reflection;
using SocialCoder.Web.Shared.Enums;

namespace SocialCoder.Web.Shared.Extensions;

public static class ExperienceItemExtensions
{
    private static readonly Dictionary<ExperienceItem, string> DisplayNameCache = new();
    private static readonly Dictionary<ExperienceItem, string> IconCache = new();
    private static int _maxId = int.MinValue;

    public static bool IsExperienceItem(this int value, out ExperienceItem item)
    {
        item = ExperienceItem.None;

        if (value < 0 || value > GetMaxInt())
        {
            return false;
        }

        item = (ExperienceItem)value;
        return true;
    }

    /// <summary>
    /// Return that maximum value of the enum suite
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static int GetMaxInt()
    {
        if (_maxId != int.MinValue)
        {
            return _maxId;
        }

        _maxId = (int)Enum.GetValues<ExperienceItem>().OrderByDescending(x => (int)x).First();
        return _maxId;
    }

    /// <summary>
    /// Retrieve Dev-branch icon for experience item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static string GetIcon(this ExperienceItem item)
    {
        if (IconCache.TryGetValue(item, out var value))
        {
            return value;
        }

        var name = $"devicon-{Enum.GetName(typeof(ExperienceItem), item)}-plain";
        IconCache.Add(item, name);
        return name;
    }

    /// <summary>
    /// Retrieve human-readable name for experience item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
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