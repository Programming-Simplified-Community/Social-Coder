using System.ComponentModel.DataAnnotations;

namespace SocialCoder.Web.Shared.Models.Setup;

public class TestReachabilityRequest
{
    [Required] public string Host { get; set; } = "localhost";
    [Range(1, 65535)] public int Port { get; set; } = 5432;
    [Required] public string UserId { get; set; }
    [Required] public string Password { get; set; }
}

public class TestConnectionRequest : TestReachabilityRequest
{
    [Required] public string Database { get; set; }
}

