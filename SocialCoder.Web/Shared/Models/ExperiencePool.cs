namespace SocialCoder.Web.Shared.Models;

/// <summary>
/// Represents various things a user may have experience in
/// </summary>
public class ExperiencePool
{
    public int Id { get; set; }
    
    /// <summary>
    /// Name of item a user can have experience in
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Image to display when rendered
    /// </summary>
    public string ImageUrl { get; set; }
}