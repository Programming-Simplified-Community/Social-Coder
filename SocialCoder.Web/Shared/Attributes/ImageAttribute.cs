namespace SocialCoder.Web.Shared.Attributes;

/// <summary>
/// Path to image in wwwroot folder (relative)
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class ImageAttribute : Attribute
{
    public string Path { get; }

    public ImageAttribute(string path)
    {
        this.Path = path;
    }
}