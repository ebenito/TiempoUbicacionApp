using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace TiempoUbicacionApp.Services
{
    /// <summary>
    /// Servicio de geolocalización.
    /// </summary>
    public class GeolocationService
    {
        private readonly HttpClient _httpClient;
        private readonly IAlertService _alertService;

        // Constructor para IHttpClientFactory (inyección por DI)
        public GeolocationService(HttpClient httpClient, IAlertService alertService)
        {
            _httpClient = httpClient;
            _alertService = alertService;
        }

        public async Task<LocationResult> GetCurrentLocationAsync(bool forceNew = false)
        {
#if ANDROID
            if (!await EnsureLocationPermissionAsync())
                return LocationResult.Empty;
#endif
#if WINDOWS
            if (!await EnsureWindowsLocationPermissionAsync())
                return LocationResult.Empty;
#endif

            Location? location = null;

            try
            {
                if (!forceNew)
                    location = await Geolocation.Default.GetLastKnownLocationAsync();

                if (location == null || forceNew)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                    location = await Geolocation.Default.GetLocationAsync(request);
                }

                if (location == null)
                {
                    await _alertService.ShowLongToastAsync("No se pudo obtener la ubicación actual.");
                    return new LocationResult("Ubicación no disponible", string.Empty, string.Empty, 0, 0);
                }

                var name = await GetLocationNameAsync(location.Latitude, location.Longitude);

                return new LocationResult(
                    Name: name,
                    FormattedLatitude: ConvertToDMS(location.Latitude, isLatitude: true),
                    FormattedLongitude: ConvertToDMS(location.Longitude, isLatitude: false),
                    Lat: location.Latitude,
                    Lng: location.Longitude);
            }
            catch (Exception ex)
            {
                await _alertService.ShowLongToastAsync($"Error: {ex.Message}");
                return new LocationResult(
                    "Error al obtener datos", "...", "...",
                    location?.Latitude ?? 0,
                    location?.Longitude ?? 0);
            }
        }

        // ── Helpers privados ─────────────────────────────────────────────────

        private async Task<string> GetLocationNameAsync(double lat, double lon)
        {
            try
            {
                string url = string.Create(
                    System.Globalization.CultureInfo.InvariantCulture,
                    $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={lat}&lon={lon}");

                var response = await _httpClient.GetFromJsonAsync<NominatimResponse>(url);

                if (response?.Address != null)
                {
                    response.Address.TryGetValue("city", out var city);
                    response.Address.TryGetValue("state", out var state);
                    if (!string.IsNullOrEmpty(city))
                        return $"{city}, {state}";
                }

                return response?.DisplayName ?? "Ubicación desconocida";
            }
            catch
            {
                return "Ubicación desconocida";
            }
        }

#if ANDROID
        private async Task<bool> EnsureLocationPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
                    await _alertService.ShowToastAsync("Se necesita acceso a la ubicación para mostrar el mapa.");

                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted)
            {
                await _alertService.ShowToastAsync("Permiso de ubicación denegado.");
                return false;
            }

            return true;
        }
#endif

        #if WINDOWS
                private async Task<bool> EnsureWindowsLocationPermissionAsync()
                {
                    try
                    {
                        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                        if (status == PermissionStatus.Granted)
                            return true;

                        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                        if (status == PermissionStatus.Granted)
                            return true;

                        await _alertService.ShowLongToastAsync(
                            "Permiso de ubicación denegado. Actívalo en Configuración > Privacidad > Ubicación.");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        await _alertService.ShowLongToastAsync($"No se puede acceder a la ubicación: {ex.Message}");
                        return false;
                    }
                }
        #endif

                private static string ConvertToDMS(double coordinate, bool isLatitude)
        {
            var degrees = (int)coordinate;
            var minutes = (int)((coordinate - degrees) * 60);
            var seconds = (int)((((coordinate - degrees) * 60) - minutes) * 60);

            var direction = isLatitude
                ? (degrees >= 0 ? "N" : "S")
                : (degrees >= 0 ? "E" : "O");

            return $"{Math.Abs(degrees)}°{Math.Abs(minutes)}'{Math.Abs(seconds)}\" {direction}";
        }
    }

    // ── Value object para el resultado ──────────────────────────────────────

    /// <summary>
    /// Sustituye la tupla anónima por un record tipado. Más legible y seguro.
    /// </summary>
    public record LocationResult(
        string Name,
        string FormattedLatitude,
        string FormattedLongitude,
        double Lat,
        double Lng)
    {
        public static LocationResult Empty =>
            new(string.Empty, string.Empty, string.Empty, 0, 0);
    }
}

// ── Modelos de Nominatim ─────────────────────────────────────────────────────

public class NominatimResponse
{
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("address")]
    public Dictionary<string, string>? Address { get; set; }
}
