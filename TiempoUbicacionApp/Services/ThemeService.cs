using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MudBlazor;
using Colors = MudBlazor.Colors;


namespace TiempoUbicacionApp.Services
{

    public class ThemeService
    {
        private bool _initialized = false;
        public bool IsInitialized => _initialized;

        public bool IsDarkMode { get; private set; }

        public MudTheme CurrentTheme => IsDarkMode ? DarkTheme : LightTheme;

        public event Action OnThemeChanged;

        private readonly ISettingsService _settingsService;

        public ThemeService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task InitializeAsync()
        {
            IsDarkMode = await _settingsService.GetIsDarkModeAsync();
            _initialized = true;
            OnThemeChanged?.Invoke();
        }

        public async Task SetDarkModeAsync(bool isDark)
        {
            IsDarkMode = isDark;
            await _settingsService.SetIsDarkModeAsync(isDark);
            OnThemeChanged?.Invoke();
        }

        private static readonly MudTheme LightTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = Colors.Blue.Default,
                Background = Colors.Gray.Lighten5
            }
        };

        private static readonly MudTheme DarkTheme = new MudTheme()
        {
            PaletteDark = new PaletteDark()
            {
                Primary = Colors.Blue.Lighten3,
                Background = Colors.Gray.Darken4
            }
        };
    }

}
