using SocialCoder.Web.Shared.Models.CodeJam;

namespace SocialCoder.Web.Shared.ViewModels.CodeJam;

/// <summary>
/// View Model for displaying Topic Information
/// </summary>
public class CodeJamViewModel
{
    /// <summary>
    /// Topic associated with this view
    /// </summary>
    public CodeJamTopic Topic { get; set; }

    /// <summary>
    /// Is the current user registered for this topic
    /// </summary>
    public bool IsRegistered { get; set; }

    public string RegisterUrl => string.Format(Endpoints.CodeJamPostTopicRegister, this.Topic.Id);
    public string AbandonUrl => string.Format(Endpoints.CodeJamPostTopicWithdraw, this.Topic.Id);

    /// <summary>
    /// # of applicants wanting a team
    /// </summary>
    public int TotalTeamApplicants { get; set; }

    /// <summary>
    /// # of solo applicants
    /// </summary>
    public int TotalSoloApplicants { get; set; }

    /// <summary>
    /// Applicants in total
    /// </summary>
    public int TotalApplicants => this.TotalSoloApplicants + this.TotalTeamApplicants;
}