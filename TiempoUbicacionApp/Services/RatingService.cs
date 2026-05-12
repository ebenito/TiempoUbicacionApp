using Microsoft.Maui.Storage;

namespace TiempoUbicacionApp.Services
{
    /// <summary>
    /// Gestiona cuándo mostrar el diálogo de valoración y recuerda la decisión del usuario.
    /// Reglas: aparece tras 3 usos de la app, nunca más si el usuario ya respondió.
    /// </summary>
    public class RatingService
    {
        // ── Claves de Preferences ────────────────────────────────────────────
        private const string KeyLaunchCount    = "rating_launch_count";
        private const string KeyAlreadyRated   = "rating_already_rated";
        private const string KeyNeverAsk       = "rating_never_ask";
        private const string KeyPostponedUntil = "rating_postponed_until";

        private const int LaunchThreshold = 3; // Número de usos antes de preguntar

        // ── Fuente de la instalación en Android ──────────────────────────────
        // Se detecta en tiempo de ejecución; también puede ser sobreescrita
        // por el usuario si lo indica (guardado en Preferences).
        private const string KeyAndroidStore = "rating_android_store"; // "play" | "amazon"

        public enum AndroidStore { Unknown, Play, Amazon }

        // ── Incrementa el contador de uso ────────────────────────────────────
        public void RecordLaunch()
        {
            var count = Preferences.Get(KeyLaunchCount, 0);
            Preferences.Set(KeyLaunchCount, count + 1);
        }

        // ── ¿Debería mostrarse el diálogo ahora? ────────────────────────────
        public bool ShouldShowRating()
        {
            if (Preferences.Get(KeyAlreadyRated, false)) return false;
            if (Preferences.Get(KeyNeverAsk, false))     return false;

            // Respeto de "recordar más tarde" (postponed 7 días)
            var postponedTicks = Preferences.Get(KeyPostponedUntil, 0L);
            if (postponedTicks > 0 && DateTime.Now.Ticks < postponedTicks) return false;

            var count = Preferences.Get(KeyLaunchCount, 0);
            return count >= LaunchThreshold;
        }

        // ── Acciones del usuario ─────────────────────────────────────────────
        public void MarkAsRated()         => Preferences.Set(KeyAlreadyRated, true);
        public void MarkAsNeverAsk()      => Preferences.Set(KeyNeverAsk, true);
        public void PostponeWeek()
        {
            var until = DateTime.Now.AddDays(7).Ticks;
            Preferences.Set(KeyPostponedUntil, until);
        }

        // ── Tienda en Android ────────────────────────────────────────────────
        public AndroidStore GetAndroidStore()
        {
            // 1. ¿Ya lo guardamos anteriormente?
            var saved = Preferences.Get(KeyAndroidStore, string.Empty);
            if (saved == "play")   return AndroidStore.Play;
            if (saved == "amazon") return AndroidStore.Amazon;

            // 2. Intentar detectar el instalador (Android API)
#if ANDROID
            try
            {
                var context  = Android.App.Application.Context;
                var pm       = context.PackageManager;
                var pkgName  = context.PackageName ?? string.Empty;

                // API 30+ usa GetInstallSourceInfo; versiones anteriores usan el método deprecado
                string? installer = null;
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.R)
                {
                    var info = pm?.GetInstallSourceInfo(pkgName);
                    installer = info?.InstallingPackageName;
                }
                else
                {
#pragma warning disable CA1422
                    installer = pm?.GetInstallerPackageName(pkgName);
#pragma warning restore CA1422
                }

                if (installer != null)
                {
                    if (installer.Contains("amazon", StringComparison.OrdinalIgnoreCase))
                    {
                        Preferences.Set(KeyAndroidStore, "amazon");
                        return AndroidStore.Amazon;
                    }
                    // com.android.vending = Google Play
                    Preferences.Set(KeyAndroidStore, "play");
                    return AndroidStore.Play;
                }
            }
            catch { /* ignorar: sideload o emulador */ }
#endif
            return AndroidStore.Unknown;
        }

        public void SetAndroidStore(AndroidStore store)
        {
            Preferences.Set(KeyAndroidStore, store == AndroidStore.Amazon ? "amazon" : "play");
        }

        // ── URLs de valoración ───────────────────────────────────────────────
        public string GetStoreUrl(AndroidStore androidStore = AndroidStore.Unknown)
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
                return "ms-windows-store://review/?ProductId=9NR9DNFKM0PC";

            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                return androidStore == AndroidStore.Amazon
                    ? "amzn://apps/android?asin=XXXXXXXXXX"       // sustituir por ASIN real
                    : "market://details?id=com.tubkala.tiempoubicacionapp";
            }

            // Fallback web
            return "https://apps.microsoft.com/detail/9NR9DNFKM0PC";
        }
    }
}
