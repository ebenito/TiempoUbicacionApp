using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiempoUbicacionApp.Services
{
    public enum MapProvider
    {
        Google = 0,
        OpenStreetMap = 1
    }


    public interface ISettingsService
    {
        Task<bool> GetIsDarkModeAsync();
        Task SetIsDarkModeAsync(bool isDark);

        Task<int> GetRefreshIntervalAsync();
        Task SetRefreshIntervalAsync(int seconds);

        Task<MapProvider> GetMapProviderAsync();
        Task SetMapProviderAsync(MapProvider provider);

    }
}
