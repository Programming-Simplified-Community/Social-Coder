namespace SocialCoder.Web.Client.Models;

public class CursorItem<TItem>
{
    public bool IsModifying { get; set; }
    public TItem Data { get; set; }
    public string Cursor { get; set; }
}