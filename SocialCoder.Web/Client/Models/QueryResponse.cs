using StrawberryShake;

namespace SocialCoder.Web.Client.Models;

public class QueryResponse<TEntity>
{
    /// <summary>
    /// Items returned from Query
    /// </summary>
    public IList<ICursorItem<TEntity>> Items { get; set; }

    public IReadOnlyList<IClientError>? Errors { get; set; }

    public HotChocolate.Types.Pagination.PageInfo PageInfo { get; set; }
}