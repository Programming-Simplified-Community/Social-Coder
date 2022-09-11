namespace SocialCoder.Web.Client.GraphQL;

public class CodeJamTopic_AdminView
{
    public int Id { get; set; }
    public int TotalApplicants { get; set; }
    public int SoloApplicants { get; set; }
    public int TeamApplicants { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime JamStartDate { get; set; }
    public DateTime JamEndDate { get; set; }
    public DateTime RegistrationStartDate { get; set; }
    public string BackgroundImageUrl { get; set; }
}

public class CodeJamTopic_UserView : CodeJamTopic_AdminView
{
    public bool IsRegistered { get; set; }
}

public interface IPagedResponse<TItem>
{
    bool HasNextPage { get; }
    bool HasPreviousPage { get; }

    ICollection<TItem> Items { get; }
}