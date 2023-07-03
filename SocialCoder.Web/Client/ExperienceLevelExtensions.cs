using System.Diagnostics;

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

    public static ExperienceLevel Translate(this Level level)
        => level switch
        {
            Level.Black => ExperienceLevel.Black,
            Level.Blue => ExperienceLevel.Blue,
            Level.Green => ExperienceLevel.Green,
            Level.Red => ExperienceLevel.Red,
            Level.Yellow => ExperienceLevel.Yellow,
            _ => ExperienceLevel.White
        };

    public static Level Translate(this ExperienceLevel level)
        => level switch
        {
            ExperienceLevel.Black => Level.Black,
            ExperienceLevel.Blue => Level.Blue,
            ExperienceLevel.Green => Level.Green,
            ExperienceLevel.Red => Level.Red,
            ExperienceLevel.Yellow => Level.Yellow,
            _ => Level.White
        };
    
    public static string Display(this Level level)
        => $"{level.ToString()} Belt ({level.GetSigmaYears()})";
}