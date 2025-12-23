using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TiempoUbicacionApp.Services;
using System.Net.Http.Json;

namespace TiempoUbicacionApp.Services
{
    public class GeolocationService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly IAlertService _alertService;

        public GeolocationService(IAlertService alertService)
        {
            _alertService = alertService;
        }

       

        public async Task<(string Name, string FormattedLatitude, string FormattedLongitude, double Lat, double Lng)>
        GetCurrentLocationAsync(bool forceNew = false)
        {
            #if ANDROID
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
                {
                    await _alertService.ShowToastAsync("Se necesita acceso a la ubicación para mostrar el mapa.");
                }

                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted)
            {
                await _alertService.ShowToastAsync("Permiso de ubicación denegado.");
                return (string.Empty, string.Empty, string.Empty, 0, 0);
            }
#endif

            Location location = null;

            try
            {
                if (!forceNew)
                {
                    location = await Geolocation.Default.GetLastKnownLocationAsync();
                }

                if (location == null || forceNew)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                    location = await Geolocation.Default.GetLocationAsync(request);
                }

                if (location == null)
                {
                    await _alertService.ShowLongToastAsync("No se pudo obtener la ubicación actual.");
                    return ("Ubicación no disponible", string.Empty, string.Empty, 0, 0);
                }

                // CORRECCIÓN: Forzamos CultureInfo.InvariantCulture para evitar la coma decimal
                string url = string.Create(System.Globalization.CultureInfo.InvariantCulture,
                    $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={location.Latitude}&lon={location.Longitude}");

                if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                {
                    // Es genial que uses info@tubkala.com, ayuda a que OSM no bloquee tu app
                    _httpClient.DefaultRequestHeaders.Add("User-Agent", "TiempoUbicacionApp/1.0 (info@tubkala.com)");
                }

                var response = await _httpClient.GetFromJsonAsync<NominatimResponse>(url);

                // Opcional: Limpiar el nombre para que no sea excesivamente largo en el MudCard
                string name = response?.DisplayName ?? "Ubicación desconocida";
                if (response?.Address != null)
                {
                    // Ejemplo: "Madrid, Comunidad de Madrid"
                    response.Address.TryGetValue("city", out var city);
                    response.Address.TryGetValue("state", out var state);
                    if (!string.IsNullOrEmpty(city)) name = $"{city}, {state}";
                }

                return (
                    Name: name,
                    FormattedLatitude: ConvertToDMS(location.Latitude, true),
                    FormattedLongitude: ConvertToDMS(location.Longitude, false),
                    Lat: location.Latitude,
                    Lng: location.Longitude
                );
            }
            catch (Exception ex)
            {
                // Importante: Si 'location' es null aquí, el return fallaría. 
                // Usamos valores por defecto si no llegamos a obtener la ubicación.
                double fallbackLat = location?.Latitude ?? 0;
                double fallbackLng = location?.Longitude ?? 0;

                await _alertService.ShowLongToastAsync($"Error: {ex.Message}");
                return ("Error al obtener datos", "...", "...", fallbackLat, fallbackLng);
            }
        }


        public async Task<(string Name, string FormattedLatitude, string FormattedLongitude, double Lat, double Lng)> GetCurrentLocationMicrosoftMapsAsync(bool forceNew = false)
        {
        #if ANDROID
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
                {
                    await _alertService.ShowToastAsync("Se necesita acceso a la ubicación para mostrar el mapa.");
                }

                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted)
            {
                await _alertService.ShowToastAsync("Permiso de ubicación denegado.");
                return (string.Empty, string.Empty, string.Empty, 0, 0);
            }
        #endif

            try
            {
                Location location = null;

                if (!forceNew)
                {
                    location = await Geolocation.Default.GetLastKnownLocationAsync();
                }
                else
                { 
                    var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                    location = await Geolocation.Default.GetLocationAsync(request);
                }                    

                if (location == null)
                {
                    await _alertService.ShowLongToastAsync("No se pudo obtener la ubicación actual.");
                    return (string.Empty, string.Empty, string.Empty, 0, 0);
                }

                var placemark = (await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude)).FirstOrDefault();
                string name = placemark != null ? $"{placemark.Locality}, {placemark.FeatureName} ({placemark.AdminArea})" : "Ubicación desconocida";

                return (
                    Name: name,
                    FormattedLatitude: ConvertToDMS(location.Latitude, true),
                    FormattedLongitude: ConvertToDMS(location.Longitude, false),
                    Lat: location.Latitude,
                    Lng: location.Longitude
                );
            }
            catch (Exception ex)
            {
                await _alertService.ShowLongToastAsync($"Error al obtener ubicación: {ex.Message}");
                return (string.Empty, string.Empty, string.Empty, 0, 0);
            }
        }


        //        public async Task<(string Name, string FormattedLatitude, string FormattedLongitude, double Lat, double Lng)> GetCurrentLocationAsync()
        //        {
        //            try
        //            {
        //#if ANDROID
        //                // 1. Verificar y solicitar permisos en Android
        //                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        //                if (status != PermissionStatus.Granted)
        //                {
        //                    if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
        //                    {
        //                        await _alertService.ShowToastAsync("Se necesita acceso a la ubicación para mostrar el mapa.");
        //                    }

        //                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        //                }

        //                if (status != PermissionStatus.Granted)
        //                {
        //                    await _alertService.ShowLongToastAsync("Permiso de ubicación denegado.");
        //                    return (string.Empty, string.Empty, string.Empty, 0, 0);
        //                }
        //#endif

        //                GeolocationAccuracy accuracy;
        //                TimeSpan timeout;

        //                var lastLocation = await Geolocation.Default.GetLastKnownLocationAsync();

        //                if (lastLocation == null)
        //                {
        //                    accuracy = GeolocationAccuracy.Best;
        //                    timeout = TimeSpan.FromSeconds(5);
        //                }
        //                else
        //                {
        //                    accuracy = GeolocationAccuracy.Medium;
        //                    timeout = TimeSpan.FromSeconds(2);
        //                }

        //                var request = new GeolocationRequest(accuracy, timeout);
        //                var location = await Geolocation.Default.GetLocationAsync(request);

        //                if (location == null)
        //                {
        //                    await _alertService.ShowLongToastAsync("⚠️ No se pudo obtener la ubicación.");
        //                    return default;
        //                }

        //                var placemark = (await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude))
        //                                .FirstOrDefault();

        //                string name = placemark != null
        //                    ? $"{placemark.Locality} ({placemark.AdminArea})"
        //                    : "Ubicación desconocida";

        //                return (
        //                    Name: name,
        //                    FormattedLatitude: ConvertToDMS(location.Latitude, true),
        //                    FormattedLongitude: ConvertToDMS(location.Longitude, false),
        //                    Lat: location.Latitude,
        //                    Lng: location.Longitude
        //                );
        //            }
        //            catch (PermissionException)
        //            {
        //                await _alertService.ShowLongToastAsync("⚠️ Permiso denegado para acceder a la ubicación.");
        //                return default;
        //            }
        //            catch (Exception ex)
        //            {
        //                await _alertService.ShowLongToastAsync($"❌ Error al obtener ubicación: {ex.Message}");
        //                return default;
        //            }
        //        }


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

        //private string ConvertToDMS(double coord, bool isLat)
        //{
        //    var dir = coord >= 0 ? (isLat ? "N" : "E") : (isLat ? "S" : "O");
        //    coord = Math.Abs(coord);
        //    int deg = (int)coord;
        //    int min = (int)((coord - deg) * 60);
        //    int sec = (int)((coord - deg - min / 60.0) * 3600);
        //    return $"{deg}º {min}' {sec}'' {dir}";
        //}
    }

}



public class NominatimResponse
{
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; }

    [JsonPropertyName("address")]
    public Dictionary<string, string> Address { get; set; }
}