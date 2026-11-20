using MudBlazor;

namespace Ledgerline.Web;

public static class LedgerlineTheme
{
    private static readonly string[] FontStack = ["Inter", "system-ui", "sans-serif"];

    public static MudTheme Create() => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#0F5E5A",
            Secondary = "#66645E",
            Success = "#17613A",
            Error = "#B3261E",
            Warning = "#8A5300",
            Info = "#1D5F8A",
            Background = "#F7F6F3",
            Surface = "#FFFFFF",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#1B1B19",
            TextPrimary = "#1B1B19",
            TextSecondary = "#66645E",
            LinesDefault = "#E6E4DE",
            LinesInputs = "#85837D",
            Divider = "#E6E4DE"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#4DB3AC",
            Secondary = "#A3A19A",
            PrimaryContrastText = "#0B1F1E",
            SecondaryContrastText = "#151514",
            Success = "#6FCF97",
            Error = "#FF8A80",
            Warning = "#F2B84B",
            Info = "#86BDE8",
            Background = "#151514",
            Surface = "#1E1E1C",
            DrawerBackground = "#1E1E1C",
            DrawerText = "#ECEAE4",
            TextPrimary = "#ECEAE4",
            TextSecondary = "#A3A19A",
            LinesDefault = "#2E2E2B",
            LinesInputs = "#8E8C85",
            Divider = "#2E2E2B"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
            DrawerWidthLeft = "220px"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = FontStack },
            H4 = new H4Typography { FontFamily = FontStack, FontSize = "1.75rem", FontWeight = "600" }
        }
    };
}
