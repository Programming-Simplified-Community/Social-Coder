using System.ComponentModel.DataAnnotations;

namespace SocialCoder.Web.Shared.ViewModels;

public class RegisterParameters
{
    [Required] 
    public string UserName { get; set; }
    
    [Required, EmailAddress] 
    public string Email { get; set; }
    
    [Required, DataType(DataType.Password)] 
    public string Password { get; set; }

    [Required, Compare(nameof(Password)), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }
}