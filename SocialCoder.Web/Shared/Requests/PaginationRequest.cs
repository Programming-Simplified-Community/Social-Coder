namespace SocialCoder.Web.Shared.Requests;

public class PaginationRequest : IPagination
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool IsDescending { get; set; }
}

public class PaginatedResponse<TItem> : PaginationRequest, IPaginatedView<TItem>
{
    public int TotalPages { get; init; }
    public int TotalRecords { get; init; }
    public ICollection<TItem> Items { get; init; } = Array.Empty<TItem>();
}