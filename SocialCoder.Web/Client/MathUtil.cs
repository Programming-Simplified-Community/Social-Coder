namespace SocialCoder.Web.Client;

public static class MathUtil
{
    /// <summary>
    /// Normalizes value between 0 and 1
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static double GetNormalizedPercentage(int value, int min, int max)
        => (double)(value - min) / (max - min);
}