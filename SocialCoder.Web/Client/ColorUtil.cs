using MudBlazor;
using MudBlazor.Utilities;

namespace SocialCoder.Web.Client;

public static class ColorUtil
{
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