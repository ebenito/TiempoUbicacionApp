using System.Diagnostics;
using TiempoUbicacionApp.Services;

namespace TiempoUbicacionApp
{
    public partial class App : Application
    {
        private readonly ThemeService _themeService;

        public App(ThemeService themeService)
        {
            InitializeComponent();
            _themeService = themeService;

            // Inicializa asincrónicamente después de que la app se haya creado
            Task.Run(async () => await _themeService.InitializeAsync());

            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
            TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "TiempoUbicacionApp" };
        }

        private void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Debug.WriteLine($"[UNHANDLED] {ex?.Message}");
        }

        private void HandleUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Debug.WriteLine($"[UNOBSERVED] {e.Exception?.Message}");
            e.SetObserved();
        }
    }

}
