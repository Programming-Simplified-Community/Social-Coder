namespace SocialCoder.Web.Shared.Requests;

public interface IPagination
{
    /// <summary>
    /// Current Page the user is on 
    /// </summary>
    int PageNumber { get; set; }
    
    /// <summary>
    /// Amount of records to view per page
    /// </summary>
    int PageSize { get; set; }

    /// <summary>
    /// Is the result set in descending order?
    /// </summary>
    bool IsDescending { get; set; }
}

public interface IPaginatedView<TItem> : IPagination
{
    /// <summary>
    /// Total number of pages the user can cycle through based on ViewSize
    /// </summary>
    int TotalPages { get; }
    
    /// <summary>
    /// Total number of records in the DB
    /// </summary>
    int TotalRecords { get; }
    
    /// <summary>
    /// Items included in this paginated response
    /// </summary>
    ICollection<TItem> Items { get; }
}