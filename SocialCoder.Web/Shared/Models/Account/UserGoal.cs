using SocialCoder.Web.Shared.Enums;

namespace SocialCoder.Web.Shared.Models.Account;

public class UserGoal
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to User
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Title that shall appear in UI
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// Possible description of what this goal entails
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// What type of goal is this?
    /// </summary>
    public GoalType GoalType { get; set; }
    
    /// <summary>
    /// Time in which this goal was created
    /// </summary>
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Target date towards achieving this goal
    /// </summary>
    public DateTime TargetDate { get; set; }
    
    /// <summary>
    /// Time in which this goal was completed
    /// </summary>
    public DateTime? CompletedOn { get; set; }
}