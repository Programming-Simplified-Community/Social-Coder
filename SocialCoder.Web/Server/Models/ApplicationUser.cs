using Microsoft.AspNetCore.Identity;

namespace SocialCoder.Web.Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Amount of experience this user has (from whichever gamification path)
        /// </summary>
        public int Experience { get; set; }
    }
}