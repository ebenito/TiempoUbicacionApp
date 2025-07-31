using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiempoUbicacionApp.Services
{
    public interface ISettingsService
    {
        Task<bool> GetIsDarkModeAsync();
        Task SetIsDarkModeAsync(bool isDark);

        Task<int> GetRefreshIntervalAsync();
        Task SetRefreshIntervalAsync(int seconds);
    }
}
