using SocialCoder.Web.Shared.Models.CodeJam;

namespace SocialCoder.Web.Shared.ViewModels.CodeJam;

public class CodeJamAdminViewModel
{
    /// <summary>
    /// Topic associated with this view
    /// </summary>
    public CodeJamTopic Topic { get; set; }

    public int TotalTeamApplicants { get; set; }
    public int TotalSoloApplicants { get; set; }
    
    public int TotalApplicants => TotalSoloApplicants + TotalTeamApplicants;
}