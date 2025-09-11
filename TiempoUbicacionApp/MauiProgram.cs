using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using TiempoUbicacionApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;



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

            builder.Services.AddHttpClient(); // para el HttpClient de DI (Dependency Injection)

            // Cargar secretos
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();


#if ANDROID           
            var assembly = Assembly.GetExecutingAssembly(); 
            using var stream = assembly.GetManifestResourceStream("appsettings.android.json");
            builder.Configuration.AddJsonStream(stream);

            //builder.Configuration.AddJsonFile("appsettings.android.json", optional: true, reloadOnChange: true);
#endif

#if WINDOWS
            builder.Configuration.AddJsonFile("appsettings.windows.json", optional: true, reloadOnChange: true);
#endif

            // Servicio OneDrive
            builder.Services.AddSingleton<IOneDriveService>(sp =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var clientId = cfg["Graph:ClientId"];
                var clientSecret = cfg["Graph:ClientSecret"];
                var tenantId = cfg["Graph:TenantId"] ?? "common";

                if (string.IsNullOrEmpty(clientId))
                    throw new InvalidOperationException("ClientId de OneDrive no está configurado en appsettings.json");

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
