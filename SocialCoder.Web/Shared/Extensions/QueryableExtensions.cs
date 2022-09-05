using System.Linq.Expressions;
using SocialCoder.Web.Shared.Requests;

namespace SocialCoder.Web.Shared.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// Paginates a response based on values provided by the <paramref name="request"/>.
    /// </summary>
    /// <param name="query">The Queryable source to utilize</param>
    /// <param name="request">Pagination request from user (or default if not provided)</param>
    /// <param name="keySelector">The column to order by</param>
    /// <typeparam name="T">Entity type we're working with</typeparam>
    /// <typeparam name="TKey">The key/column type to sort on</typeparam>
    /// <returns><see cref="IQueryable{T}"/> that is paginated</returns>
    public static IQueryable<T> PaginatedQuery<T, TKey>(this IQueryable<T> query, 
        PaginationRequest request, 
        Expression<Func<T,TKey>> keySelector)
    {
        query = request.IsDescending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
        
        return query.Skip(request.PageNumber * request.PageSize).Take(request.PageSize);
    }
}