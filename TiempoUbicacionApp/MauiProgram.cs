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
            builder.Services.AddMudServices();

            IServiceCollection serviceCollection = builder.Services.AddSingleton<IAlertService, MauiAlertService>();
            builder.Services.AddSingleton<LocationDatabaseService>();
            builder.Services.AddSingleton<GeolocationService>();


#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
