using System.ComponentModel.DataAnnotations;

namespace SocialCoder.Web.Shared.ViewModels;

public class LoginParameters
{
    [Required] public string UserName { get; set; }
    [Required, DataType(DataType.Password)] public string Password { get; set; }
    public bool RememberMe { get; set; }
}