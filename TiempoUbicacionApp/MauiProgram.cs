using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using TiempoUbicacionApp.Platforms;
using TiempoUbicacion.Shared.Services;

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

            // Registro de servicios concretos para interfaces compartidas
            builder.Services.AddSingleton<IAlertService, MauiAlertService>();
            builder.Services.AddSingleton<IGeolocationService, MauiGeolocationService>();   
            builder.Services.AddSingleton<ILocationDatabaseService, MauiLocationDatabaseService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
