using HotChocolate;
using HotChocolate.Types;

namespace SocialCoder.Web.Shared.Models.CodeJam;

public class CodeJamRegistration
{
    [GraphQLType(typeof(IdType))]
    public int Id { get; set; }
    
    /// <summary>
    /// Foreign key to User
    /// </summary>
    public string UserId { get; set; }
    
    /// <summary>
    /// Foreign Key to <see cref="CodeJamTopic"/>
    /// </summary>
    public int CodeJamTopicId { get; set; }

    /// <summary>
    /// Time in which the user registered for this code jam
    /// </summary>
    public DateTime RegisteredOn { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Time in which the user left the jam (if applicable)
    /// </summary>
    public DateTime? AbandonedOn { get; set; }

    /// <summary>
    /// User wants to be on a team
    /// </summary>
    public bool PreferTeam { get; set; } = true;

    public CodeJamTopic CodeJamTopic { get; set; }
}