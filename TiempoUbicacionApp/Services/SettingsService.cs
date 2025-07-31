using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace TiempoUbicacionApp.Services
{
    public class MauiSettingsService : ISettingsService
    {
        private const string DarkModeKey = "temaOscuro";
        private const string RefreshIntervalKey = "intervaloRefresco";

        public Task<bool> GetIsDarkModeAsync()
        {
            return Task.FromResult(Preferences.Get(DarkModeKey, false));
        }

        public Task SetIsDarkModeAsync(bool isDark)
        {
            Preferences.Set(DarkModeKey, isDark);
            return Task.CompletedTask;
        }

        public Task<int> GetRefreshIntervalAsync()
        {
            return Task.FromResult(Preferences.Get(RefreshIntervalKey, 30));
        }

        public Task SetRefreshIntervalAsync(int seconds)
        {
            Preferences.Set(RefreshIntervalKey, seconds);
            return Task.CompletedTask;
        }
    }
}
