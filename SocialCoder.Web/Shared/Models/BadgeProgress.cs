using HotChocolate;
using HotChocolate.Types;

#pragma warning disable CS8618
namespace SocialCoder.Web.Shared.Models;

public class BadgeProgress
{
    [GraphQLType(typeof(IdType))]
    public int Id { get; set; }
    
    /// <summary>
    /// Foreign key to <see cref="Badge"/>
    /// </summary>
    public int BadgeId { get; set; }

    /// <summary>
    /// Amount towards requirement
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// Is this badge considered complete?
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Foreign key to User
    /// </summary>
    public string UserId { get; set; }
    
    /// <summary>
    /// Navigational property (in EF)
    /// </summary>
    public Badge Badge { get; set; }
}