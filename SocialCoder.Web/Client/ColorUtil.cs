using MudBlazor;

namespace SocialCoder.Web.Client;

public static class ColorUtil
{
    public static string GetSigmaColor(this MudTheme theme, Web.Shared.Enums.ExperienceLevel level)
        => level switch
        {
            Web.Shared.Enums.ExperienceLevel.Black => "black",
            Web.Shared.Enums.ExperienceLevel.Red => "red",
            Web.Shared.Enums.ExperienceLevel.Green => theme.PaletteDark.SuccessLighten,
            Web.Shared.Enums.ExperienceLevel.Blue => "cyan",
            Web.Shared.Enums.ExperienceLevel.Yellow => "yellow",
            _ => "white"
        };
    public static string GetCssValue(MudTheme theme, Color color)
    {
        return color switch
        {
            Color.Primary => theme.PaletteDark.Primary.Value,
            Color.Secondary => theme.PaletteDark.Secondary.Value,
            Color.Tertiary => theme.PaletteDark.Tertiary.Value,
            Color.Dark => theme.PaletteDark.Dark.Value,
            Color.Error => theme.PaletteDark.Error.Value,
            Color.Surface => theme.PaletteDark.Surface.Value,
            Color.Warning => theme.PaletteDark.Warning.Value,
            Color.Info => theme.PaletteDark.Info.Value,
            Color.Success => theme.PaletteDark.Success.Value,
            _ => theme.PaletteDark.Primary.Value
        };
    }
}