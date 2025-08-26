using Microsoft.AspNetCore.Components;

namespace SocialCoder.Web.Client.Shared.Components.Topics;

public partial class JamTopic : ComponentBase
{
    [Parameter] public IGetTopicsWithUserInfo_Topics_Edges_Node Topic { get; set; }

    [CascadingParameter(Name = "UserId")] public string UserId { get; set; }

    private string? _image;
    private Dictionary<string, object> _backgroundAttributes = new();

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _image = string.IsNullOrWhiteSpace(Topic.BackgroundImageUrl)
            ? Images.RandomBackgroundImage()
            : Topic.BackgroundImageUrl;
        _image = $"background-image: url(\"{_image}\");";
    }


}