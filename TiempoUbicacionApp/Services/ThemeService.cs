using MudBlazor;

namespace TiempoUbicacionApp.Services
{
    public class ThemeService
    {
        private readonly ISettingsService _settingsService;
        public event Action<bool>? OnChange;

        public bool IsDarkMode { get; private set; }
        public MudTheme Theme { get; private set; } = new();

        public ThemeService(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        public async Task InitializeAsync()
        {
            IsDarkMode = await _settingsService.GetIsDarkModeAsync();
            Theme = BuildTheme(IsDarkMode);
            OnChange?.Invoke(IsDarkMode);
        }

        public void SetDarkMode(bool isDark)
        {
            IsDarkMode = isDark;
            Theme = BuildTheme(isDark);
            _settingsService.SetIsDarkModeAsync(isDark);
            OnChange?.Invoke(isDark);
        }

        public MudTheme GetTheme() => Theme ?? BuildTheme(IsDarkMode);

        private static MudTheme BuildTheme(bool isDark) => new MudTheme
        {
            // ── Paleta clara ────────────────────────────────────────────
            PaletteLight = new PaletteLight
            {
                Primary = "#0d9488",
                PrimaryDarken = "#0f766e",
                PrimaryLighten = "#14b8a6",
                Secondary = "#0891b2",
                Tertiary = "#059669",
                Success = "#16a34a",
                Info = "#0284c7",
                Warning = "#d97706",
                Error = "#dc2626",

                AppbarBackground = "#0f172a",
                AppbarText = "#f8fafc",

                DrawerBackground = "#0f172a",
                DrawerText = "#cbd5e1",
                DrawerIcon = "#14b8a6",

                Background = "#f0fdfa",
                BackgroundGray = "#e2e8f0",
                Surface = "#ffffff",

                TextPrimary = "#0f172a",
                TextSecondary = "#475569",
                ActionDefault = "#0d9488",

                Divider = "#e2e8f0",
                TableLines = "#e2e8f0",
            },

            // ── Paleta oscura ────────────────────────────────────────────
            PaletteDark = new PaletteDark
            {
                Primary = "#14b8a6",
                PrimaryDarken = "#0d9488",
                PrimaryLighten = "#2dd4bf",
                Secondary = "#22d3ee",
                Tertiary = "#34d399",
                Success = "#4ade80",
                Info = "#38bdf8",
                Warning = "#fbbf24",
                Error = "#f87171",

                AppbarBackground = "#020617",
                AppbarText = "#f8fafc",

                DrawerBackground = "#020617",
                DrawerText = "#94a3b8",
                DrawerIcon = "#2dd4bf",

                Background = "#0f172a",
                BackgroundGray = "#1e293b",
                Surface = "#1e293b",

                TextPrimary = "#f1f5f9",
                TextSecondary = "#94a3b8",
                ActionDefault = "#14b8a6",

                Divider = "#334155",
                TableLines = "#334155",
            },

            // ── Tipografía (MudBlazor 9.x) ────────────────────────────────
            Typography = new Typography
            {
                Default = new DefaultTypography
                {
                    FontFamily = ["DM Sans", "system-ui", "sans-serif"],
                    FontSize = "0.9375rem",
                    LineHeight = "1.6",
                },
                H5 = new H5Typography
                {
                    FontFamily = ["DM Sans", "system-ui", "sans-serif"],
                    FontSize = "1.2rem",
                    FontWeight = "600",
                    LetterSpacing = "-.01em",
                },
                H6 = new H6Typography
                {
                    FontFamily = ["DM Sans", "system-ui", "sans-serif"],
                    FontSize = "1rem",
                    FontWeight = "600",
                },
                Body1 = new Body1Typography
                {
                    FontFamily = ["DM Sans", "system-ui", "sans-serif"],
                    FontSize = "0.9375rem",
                },
                Body2 = new Body2Typography
                {
                    FontFamily = ["DM Sans", "system-ui", "sans-serif"],
                    FontSize = "0.875rem",
                },
                Subtitle2 = new Subtitle2Typography
                {
                    FontFamily = ["DM Sans", "system-ui", "sans-serif"],
                    FontSize = "0.75rem",
                    FontWeight = "600",
                    LetterSpacing = ".06em",
                },
            },

            // ── Layout ────────────────────────────────────────────────────
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "10px",
                DrawerWidthLeft = "260px",
            }
        };
    }
}