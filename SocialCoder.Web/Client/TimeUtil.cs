namespace SocialCoder.Web.Client;

public static class TimeUtil
{
    public static int TimezoneOffset { get; set; }

    public static DateTime ToRealLocalTime(this DateTime dateTime)
        => dateTime.AddMinutes(TimezoneOffset);

    public static DateTimeOffset ToRealLocalTime(this DateTimeOffset dateTime)
        => dateTime.AddMinutes(TimezoneOffset);
}