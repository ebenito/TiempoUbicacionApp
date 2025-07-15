using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiempoUbicacionApp.Services
{
    public class GeolocationService
    {
        public async Task<(string Name, string FormattedLatitude, string FormattedLongitude, double Lat, double Lng)> GetCurrentLocationAsync()
        {
            var location = await Geolocation.Default.GetLastKnownLocationAsync();
            if (location == null)
                location = await Geolocation.Default.GetLocationAsync();

            var placemark = (await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude)).FirstOrDefault();

            return (
                Name: placemark?.Locality + " (" + placemark?.AdminArea + ")",
                FormattedLatitude: ConvertToDMS(location.Latitude, true),
                FormattedLongitude: ConvertToDMS(location.Longitude, false),
                Lat: location.Latitude,
                Lng: location.Longitude
            );
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
