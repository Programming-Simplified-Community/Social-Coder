using System.Reflection;
using SocialCoder.Web.Shared.Attributes;
using SocialCoder.Web.Shared.Models;

namespace SocialCoder.Web.Shared.Extensions;

public static class ImageEnumExtensions
{
    private static Dictionary<PageType, string> _pathTypeCache = new();

    public static string? GetImagePath(this PageType imageType)
    {
        if (_pathTypeCache.ContainsKey(imageType))
            return _pathTypeCache[imageType];

        var type = imageType.GetType();
        var memberInfo = type.GetMember(imageType.ToString());
        var attributes = memberInfo.First().GetCustomAttributes<ImageAttribute>(false).ToList();

        if (!attributes.Any()) return null;
        
        var path = attributes[0].Path;
        _pathTypeCache.Add(imageType, path);
        return path;
    }
}