using MudBlazor;

namespace DataReconciliationEngine.Web.Theme;

/// <summary>
/// Centralized MudBlazor theme for the Data Reconciliation Engine.
/// </summary>
public static class AppTheme
{
    private static readonly string[] FontFamily =
        ["Inter", "Roboto", "Helvetica", "Arial", "sans-serif"];

    public static readonly MudTheme Instance = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1565C0",
            PrimaryDarken = "#0D47A1",
            PrimaryLighten = "#42A5F5",
            Secondary = "#00897B",
            SecondaryDarken = "#00695C",
            SecondaryLighten = "#4DB6AC",
            Tertiary = "#7C4DFF",
            Info = "#0288D1",
            Success = "#2E7D32",
            Warning = "#F9A825",
            Error = "#C62828",
            AppbarBackground = "#0D47A1",
            AppbarText = "#FFFFFF",
            DrawerBackground = "#FAFBFC",
            DrawerText = "#424242",
            DrawerIcon = "#616161",
            Surface = "#FFFFFF",
            Background = "#F4F6F8",
            TextPrimary = "#212121",
            TextSecondary = "#616161",
            ActionDefault = "#757575",
            ActionDisabled = "#BDBDBD",
            Divider = "#E0E0E0",
            LinesDefault = "#E0E0E0",
            HoverOpacity = 0.06,
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#42A5F5",
            PrimaryDarken = "#1E88E5",
            PrimaryLighten = "#90CAF9",
            Secondary = "#4DB6AC",
            Tertiary = "#B388FF",
            Info = "#29B6F6",
            Success = "#66BB6A",
            Warning = "#FDD835",
            Error = "#EF5350",
            AppbarBackground = "#1A1A2E",
            AppbarText = "#FFFFFF",
            DrawerBackground = "#16213E",
            DrawerText = "#E0E0E0",
            DrawerIcon = "#90CAF9",
            Surface = "#1E1E2F",
            Background = "#121212",
            TextPrimary = "#E0E0E0",
            TextSecondary = "#AAAAAA",
            ActionDefault = "#BDBDBD",
            Divider = "#333333",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = FontFamily,
                FontSize = "0.875rem",
                FontWeight = "400",
                LineHeight = "1.5",
                LetterSpacing = "0.00938em",
            },
            H4 = new H4Typography
            {
                FontFamily = FontFamily,
                FontSize = "1.5rem",
                FontWeight = "600",
                LineHeight = "1.3",
            },
            H5 = new H5Typography
            {
                FontFamily = FontFamily,
                FontSize = "1.25rem",
                FontWeight = "600",
                LineHeight = "1.3",
            },
            H6 = new H6Typography
            {
                FontFamily = FontFamily,
                FontSize = "1rem",
                FontWeight = "600",
                LineHeight = "1.4",
            },
            Subtitle1 = new Subtitle1Typography
            {
                FontFamily = FontFamily,
                FontSize = "0.875rem",
                FontWeight = "500",
            },
            Subtitle2 = new Subtitle2Typography
            {
                FontFamily = FontFamily,
                FontSize = "0.8125rem",
                FontWeight = "500",
            },
            Body1 = new Body1Typography
            {
                FontFamily = FontFamily,
                FontSize = "0.875rem",
                FontWeight = "400",
                LineHeight = "1.6",
            },
            Body2 = new Body2Typography
            {
                FontFamily = FontFamily,
                FontSize = "0.8125rem",
                FontWeight = "400",
                LineHeight = "1.5",
            },
            Button = new ButtonTypography
            {
                FontFamily = FontFamily,
                FontSize = "0.8125rem",
                FontWeight = "500",
                TextTransform = "none",
            },
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
            DrawerWidthLeft = "260px",
            DrawerMiniWidthLeft = "72px",
        },
    };
}