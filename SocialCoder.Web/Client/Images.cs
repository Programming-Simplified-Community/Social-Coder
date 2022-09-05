namespace SocialCoder.Web.Client;

public static class Images
{
    private static readonly Random Random = new();
    private static readonly string[] DefaultImages = {
        "https://images.unsplash.com/photo-1619472351888-f844a0b33f5b?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=2456&q=80",
        "https://images.unsplash.com/photo-1619472376731-3ca648a34b69?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1828&q=80",
        "https://images.unsplash.com/photo-1530714457710-6bf1899c1d32?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1740&q=80",
        "https://images.unsplash.com/photo-1428908728789-d2de25dbd4e2?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1740&q=80",
        "https://images.unsplash.com/photo-1548625150-84c33677dfe5?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1545&q=80",
        "https://images.unsplash.com/photo-1523296004693-49fb425078b4?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1752&q=80",

        "https://images.unsplash.com/photo-1527602299945-a59a63692e70?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=765&q=80",
        "https://images.unsplash.com/photo-1549948558-1c6406684186?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=687&q=80",
    };

    /// <summary>
    /// Select a random background image from <see cref="DefaultImages"/>
    /// </summary>
    /// <returns></returns>
    public static string RandomBackgroundImage()
    {
        return DefaultImages[Random.Next(0, DefaultImages.Length)];
    }
}