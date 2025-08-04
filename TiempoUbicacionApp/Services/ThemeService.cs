using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MudBlazor;
using MudBlazor.Utilities;
using Colors = MudBlazor.Colors;


namespace TiempoUbicacionApp.Services
{

    public class ThemeService
    {
        private readonly ISettingsService _settingsService;
        public event Action<bool> OnChange;

        public bool IsDarkMode { get; private set; }
        public MudTheme Theme { get; private set; }

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
            _settingsService.SetIsDarkModeAsync(isDark); // persistimos
            OnChange?.Invoke(isDark);
        }

        public MudTheme GetTheme() => Theme ?? BuildTheme(IsDarkMode);
              
        private MudTheme BuildTheme(bool isDark)
        {
            return new MudTheme
            {
                PaletteLight = new PaletteLight {
                    Primary = Colors.Cyan.Default,              // botones, switches
                    Secondary = Colors.Blue.Accent4,            // detalles, acentos
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
                    Primary = Colors.Cyan.Lighten2,
                    AppbarBackground = Colors.BlueGray.Darken3,
                    DrawerBackground = Colors.BlueGray.Darken2,   // menos oscuro
                    Background = Colors.Gray.Darken3,
                    Surface = Colors.Gray.Darken2                 // superficie más clara

                    //Primary = Colors.Teal.Lighten2,             // botones y switches
                    //Secondary = Colors.Orange.Accent3,          // acentos suaves
                    //AppbarBackground = Colors.Gray.Darken4,     // encabezado
                    //DrawerBackground = Colors.BlueGray.Darken3, // fondo del menú
                    //Background = Colors.Gray.Darken4,           // fondo general
                    //Surface = Colors.Gray.Darken3               // tarjetas
                }
            };
        }

    }


}
