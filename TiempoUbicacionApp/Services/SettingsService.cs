using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace TiempoUbicacionApp.Services
{
    public class MauiSettingsService : ISettingsService
    {
        private const string DarkModeKey = "temaOscuro";
        private const string RefreshIntervalKey = "intervaloRefresco";

        // Devuelve si el tema oscuro está activado. Por defecto: falso (tema claro)
        public Task<bool> GetIsDarkModeAsync()
        {
            return Task.FromResult(Preferences.Get(DarkModeKey, false));
        }

        // Guarda la preferencia del tema oscuro
        public Task SetIsDarkModeAsync(bool isDark)
        {
            Preferences.Set(DarkModeKey, isDark);
            return Task.CompletedTask;
        }

        // Devuelve el intervalo de refresco. Por defecto: 30 segundos
        public Task<int> GetRefreshIntervalAsync()
        {
            return Task.FromResult(Preferences.Get(RefreshIntervalKey, 30));
        }

        // Guarda el intervalo de refresco
        public Task SetRefreshIntervalAsync(int seconds)
        {
            Preferences.Set(RefreshIntervalKey, seconds);
            return Task.CompletedTask;
        }

#if DEBUG
        // Método auxiliar para borrar todas las preferencias (solo en modo depuración)
        public static void ResetPreferences()
        {
            Preferences.Clear();
        }
#endif
    }
}
