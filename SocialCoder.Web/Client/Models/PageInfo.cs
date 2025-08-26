namespace SocialCoder.Web.Client.Models;

public record PageInfoParams(
    int PageSize,
    string? Cursor
);

public interface ICursorItem<TEntity>
{
    public string Cursor { get; set; }
    public TEntity Node { get; set; }
}