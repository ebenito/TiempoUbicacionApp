using Microsoft.Extensions.DependencyInjection;
using TiempoUbicacionApp.Services;

namespace TiempoUbicacionApp.Platforms.Android;

public static class PlatformServiceRegistration
{
    public static void AddPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<IAlertService, MauiAlertService>();
    }
}
