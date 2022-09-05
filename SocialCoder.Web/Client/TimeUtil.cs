namespace SocialCoder.Web.Client;

public static class TimeUtil
{
    public static int TimezoneOffset { get; set; }

    public static DateTime ToRealLocalTime(this DateTime dateTime)
        => dateTime.AddMinutes(TimezoneOffset);
}