using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SocialCoder.Web.Shared.Models.Setup;

public class OAuthSetting
{
    [Required]
    public string Name { get; set; }

    [Required, DisplayName("Client Id")]
    public string ClientId { get; set; }

    [Required, PasswordPropertyText, DisplayName("Client Secret")]
    public string ClientSecret { get; set; }
}