namespace SocialCoder.Web.Client;
using Level = Web.Shared.Enums.ExperienceLevel;

public static class ExperienceLevelExtensions
{
    /// <summary>
    /// Textual representation of years for a <paramref name="level"/>
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public static string GetSigmaYears(this Level level)
        => level switch
        {
            Level.Yellow => "2-3 years",
            Level.Green => "4-5 years",
            Level.Blue => "6-7 years",
            Level.Red => "8-9 years",
            Level.Black => "10+ years",
            _ => "0-1 years"
        };

    public static string Display(this Level level)
        => $"{level.ToString()} Belt ({level.GetSigmaYears()})";
}