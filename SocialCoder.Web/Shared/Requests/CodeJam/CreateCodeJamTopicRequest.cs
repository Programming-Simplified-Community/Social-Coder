namespace SocialCoder.Web.Shared.Requests.CodeJam;

public class CreateCodeJamTopicRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string BackgroundImageUrl { get; set; }
    public DateTime JamStartDate { get; set; }
    public DateTime JamEndDate { get; set; }
    public DateTime JamRegistrationStart { get; set; }
}

public class UpdateCodeJamTopicRequest : CreateCodeJamTopicRequest
{
    public int TopicId { get; set; }
    public bool IsActive { get; set; } = true;
}