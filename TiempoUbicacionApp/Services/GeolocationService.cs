using System;
using System.Linq;
using System.Threading.Tasks;
using TiempoUbicacionApp.Services;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;

namespace TiempoUbicacionApp.Services
{
    public class GeolocationService
    {
        private readonly IAlertService _alertService;

        public GeolocationService(IAlertService alertService)
        {
            _alertService = alertService;
        }

        public async Task<(string Name, string FormattedLatitude, string FormattedLongitude, double Lat, double Lng)> GetCurrentLocationAsync()
        {
            try
            {
                var location = await Geolocation.Default.GetLastKnownLocationAsync();
                if (location == null)
                    location = await Geolocation.Default.GetLocationAsync();

                if (location == null)
                {
                    await _alertService.ShowToastAsync("⚠️ No se pudo obtener la ubicación.");
                    return default;
                }

                var placemark = (await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude)).FirstOrDefault();

                return (
                    Name: placemark?.Locality + " (" + placemark?.AdminArea + ")",
                    FormattedLatitude: ConvertToDMS(location.Latitude, true),
                    FormattedLongitude: ConvertToDMS(location.Longitude, false),
                    Lat: location.Latitude,
                    Lng: location.Longitude
                );
            }
            catch (PermissionException)
            {
                await _alertService.ShowToastAsync("⚠️ Permiso denegado para acceder a la ubicación.");
                return default;
            }
            catch (Exception ex)
            {
                await _alertService.ShowToastAsync($"❌ Error al obtener ubicación: {ex.Message}");
                return default;
            }
        }


        private string ConvertToDMS(double coord, bool isLat)
        {
            var dir = coord >= 0 ? (isLat ? "N" : "E") : (isLat ? "S" : "O");
            coord = Math.Abs(coord);
            int deg = (int)coord;
            int min = (int)((coord - deg) * 60);
            int sec = (int)((coord - deg - min / 60.0) * 3600);
            return $"{deg}º {min}' {sec}'' {dir}";
        }
    }

}
