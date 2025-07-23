using TiempoUbicacion.Shared.Services;
using CommunityToolkit.Maui.Alerts;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices.Sensors;
using TiempoUbicacion.Shared.Models;
using SQLite;

namespace TiempoUbicacionApp.Platforms
{
    public class MauiAlertService : IAlertService
    {
        public async Task ShowToastAsync(string message)
        {
            var toast = Toast.Make(message);
            await toast.Show();
        }
    }

    public class MauiGeolocationService : IGeolocationService
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

    public class MauiLocationDatabaseService : ILocationDatabaseService
    {
        private SQLiteAsyncConnection _db;
        public async Task InitAsync()
        {
            if (_db != null) return;
            var path = Path.Combine(FileSystem.AppDataDirectory, "locations.db");
            _db = new SQLiteAsyncConnection(path);
            await _db.CreateTableAsync<LocationEntry>();
        }

        public Task SaveEntryAsync(LocationEntry entry)
        {
            return _db.InsertAsync(entry);
        }

        public Task<List<LocationEntry>> GetAllEntriesAsync()
        {
            return _db.Table<LocationEntry>().ToListAsync();
        }
    }
}
