using System.Reflection;
using SocialCoder.Web.Shared.Attributes;
using SocialCoder.Web.Shared.Models;

namespace SocialCoder.Web.Shared.Extensions;

public static class ImageEnumExtensions
{
    private static readonly Dictionary<PageType, string> PathTypeCache = new();

    public static string? GetImagePath(this PageType imageType)
    {
        if (PathTypeCache.ContainsKey(imageType))
            return PathTypeCache[imageType];

        var type = imageType.GetType();
        var memberInfo = type.GetMember(imageType.ToString());
        var attributes = memberInfo.First().GetCustomAttributes<ImageAttribute>(false).ToList();

        if (!attributes.Any()) return null;
        
        var path = attributes[0].Path;
        PathTypeCache.Add(imageType, path);
        return path;
    }
}