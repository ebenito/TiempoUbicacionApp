using TiempoUbicacionApp.Services;

namespace TiempoUbicacionApp
{
    public partial class App : Application
    {
        private readonly ThemeService _themeService;

        public App(ThemeService themeService)
        {
            _themeService = themeService;
            InitializeComponent();

//#if DEBUG
//            MauiSettingsService.ResetPreferences();
//#endif
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "TiempoUbicacionApp" };
        }
    }
}
