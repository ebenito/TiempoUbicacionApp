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
