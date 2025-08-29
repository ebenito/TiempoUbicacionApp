using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using TiempoUbicacionApp.Services;
using Microsoft.Extensions.Configuration;

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

#if DEBUG
            // Cargar secretos solo en desarrollo
            builder.Configuration              
                .AddUserSecrets("tiempo-ubicacion-app-12345")     //.AddUserSecrets<App>()
                .AddEnvironmentVariables();
#endif

            builder.Services.AddHttpClient(); // para el HttpClient de DI

            builder.Services.AddSingleton<IOneDriveService>(sp =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var clientId = cfg["Graph:ClientId"];
                var tenantId = cfg["Graph:TenantId"] ?? "common";
                return new OneDriveService(clientId!, tenantId);
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
