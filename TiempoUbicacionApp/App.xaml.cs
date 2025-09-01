using System.Diagnostics;
using TiempoUbicacionApp.Services;

namespace TiempoUbicacionApp
{
    public partial class App : Application
    {
        private readonly ThemeService _themeService;
        private readonly LocationDatabaseService _dbService;

        public App(ThemeService themeService, LocationDatabaseService dbService)
        {
            InitializeComponent();
            _themeService = themeService;
            _dbService = dbService;

            // Inicializa asincrónicamente después de que la app se haya creado
            Task.Run(async () => await _themeService.InitializeAsync());

            // Inicializar la BD en segundo plano
            Task.Run(async () => await _dbService.InitAsync());

            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
            TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
            _dbService = dbService;
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
