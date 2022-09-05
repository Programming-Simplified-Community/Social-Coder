namespace SocialCoder.Web.Shared.Requests;

public class SpecificDateQuery : PaginationRequest, ISpecificDateQuery
{
    public DateTime? Date { get; set; }
}