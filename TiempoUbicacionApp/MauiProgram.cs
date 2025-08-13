using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using TiempoUbicacionApp.Services;

#if ANDROID
using TiempoUbicacionApp.Platforms.Android;
#elif WINDOWS
using TiempoUbicacionApp.Platforms.Windows;
#endif


namespace TiempoUbicacionApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            builder.Services.AddSingleton<ITimeZoneService, TimeZoneService>();
            builder.Services.AddSingleton<ISettingsService, MauiSettingsService>();
            builder.Services.AddSingleton<ThemeService>();
            builder.Services.AddMudServices();

            builder.Services.AddPlatformServices(); // Se agrega el registro de servicios de la plataforma (Método 1 de diversificación por plataforma)

            #if WINDOWS // Método 2 de diversificación por plataforma
                builder.Services.AddSingleton<IAlertService, MauiAlertService>(); // Usa MudBlazor
#           else
                builder.Services.AddSingleton<IAlertService, NativeAlertService>(); // Usa CommunityToolkit
            #endif

            builder.Services.AddSingleton<LocationDatabaseService>();
            builder.Services.AddSingleton<GeolocationService>();
            builder.Services.AddSingleton<App>();


#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
