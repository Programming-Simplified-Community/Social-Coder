using StrawberryShake;

namespace SocialCoder.Web.Client.Models;

public class QueryResponse<TEntity>
{
    /// <summary>
    /// Items returned from Query
    /// </summary>
    public IList<TEntity> Items { get; set; }

    public IReadOnlyList<IClientError>? Errors { get; set; }

    /// <summary>
    /// Total items in the database (not the total number of items in <see cref="Items"/>)
    /// </summary>
    public int TotalDbCount { get; set; }
}