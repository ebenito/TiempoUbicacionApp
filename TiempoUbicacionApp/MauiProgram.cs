using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Maps;
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
                .UseMauiCommunityToolkitMaps(
                    Environment.GetEnvironmentVariable("MAPS_API_KEY") ?? string.Empty)
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            // ── Servicios de dominio ─────────────────────────────────────
            builder.Services.AddSingleton<ITimeZoneService, TimeZoneService>();
            builder.Services.AddSingleton<ISettingsService, MauiSettingsService>();
            builder.Services.AddSingleton<ThemeService>();
            builder.Services.AddSingleton<LocationDatabaseService>();
            builder.Services.AddSingleton<App>();

            // ── BackupService centralizado ───────────────────────────────
            builder.Services.AddSingleton<BackupService>();

            // ── RatingService ────────────────────────────────────────────
            // Singleton: un único contador de usos en toda la sesión.
            builder.Services.AddSingleton<RatingService>();

            // ── HttpClient con IHttpClientFactory ────────────────────────
            // Mejor gestión del pool de conexiones TCP.
            builder.Services.AddHttpClient<GeolocationService>(client =>
            {
                client.DefaultRequestHeaders.Add(
                    "User-Agent",
                    "TiempoUbicacionApp/1.0 (info@tubkala.com)");
            });

            // ── MudBlazor ────────────────────────────────────────────────
            builder.Services.AddMudServices();

            // ── Servicios de plataforma ──────────────────────────────────

            // Método 1: registro específico de plataforma (IFeedbackService)
            builder.Services.AddPlatformServices();


            // Método 2: IAlertService diferente por plataforma
#if WINDOWS
            builder.Services.AddSingleton<IAlertService, MauiAlertService>();
#else
            builder.Services.AddSingleton<IAlertService, NativeAlertService>();
#endif

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}