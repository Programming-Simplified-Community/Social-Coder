namespace SocialCoder.Web.Shared;

public record ResultOf<T>(T? Data, string? Message = null, bool Success = true)
{
    public static ResultOf<T> Pass(T data) => new(data);
    public static ResultOf<T> Pass(T data, string message) => new(data, message);
    public static ResultOf<T> Fail(string message) => new(default, message, false);
}