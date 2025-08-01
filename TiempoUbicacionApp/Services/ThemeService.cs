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
        private readonly ISettingsService _settingsService;
        private bool _initialized = false;
        public bool IsInitialized => _initialized;
        public event Action OnThemeChanged;
        public event Action<bool> OnChange;
        public MudTheme Theme { get; private set; }
        public bool IsDarkMode { get; private set; }

        public void SetDarkMode(bool isDark)
        {
            IsDarkMode = isDark;
            Theme = BuildTheme(isDark);
            OnChange?.Invoke(isDark);
        }

        public async Task InitializeAsync()
        {
            //IsDarkMode = await _settingsService.GetIsDarkModeAsync();
            _initialized = true;
            OnThemeChanged?.Invoke();
        }

        private MudTheme BuildTheme(bool isDark)
        {
            return new MudTheme
            {
                PaletteLight = new PaletteLight {
                    Primary = Colors.Cyan.Default,              // botones, switches
                    Secondary = Colors.Orange.Accent2,          // detalles, acentos
                    AppbarBackground = Colors.Shades.White,     // encabezado
                    DrawerBackground = Colors.Cyan.Lighten5,    // fondo del menú
                    Background = Colors.Gray.Lighten5,          // fondo principal
                    Surface = Colors.Shades.White               // tarjetas, etc.
                    //Primary = Colors.Blue.Default,
                    //AppbarBackground = Colors.Blue.Lighten1,
                    //DrawerBackground = Colors.Blue.Lighten5,
                    //Background = Colors.Gray.Lighten5
                },
                PaletteDark = new PaletteDark {
                    Primary = Colors.Teal.Lighten2,             // botones y switches
                    Secondary = Colors.Orange.Accent3,          // acentos suaves
                    AppbarBackground = Colors.Gray.Darken4,     // encabezado
                    DrawerBackground = Colors.BlueGray.Darken3, // fondo del menú
                    Background = Colors.Gray.Darken4,           // fondo general
                    Surface = Colors.Gray.Darken3               // tarjetas
                }
            };
        }

        public MudTheme GetTheme() => Theme ?? BuildTheme(IsDarkMode);
    }


}
