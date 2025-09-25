using SQLite;
using TiempoUbicacionApp.Models;

namespace TiempoUbicacionApp.Services
{
    public class LocationDatabaseService
    {
        private SQLiteAsyncConnection _db;
        private readonly string _dbPath;

        public bool IsOpen => _db != null;

        public LocationDatabaseService()
        {
            _dbPath = Path.Combine(FileSystem.AppDataDirectory, "Ubicaciones.db");
        }

        public async Task InitAsync()
        {
            if (_db == null)
            {
                _db = new SQLiteAsyncConnection(_dbPath);
                await _db.CreateTableAsync<LocationEntry>();
            }
        }

        public async Task OpenAsync()
        {
            if (_db == null)
            {
                _db = new SQLiteAsyncConnection(_dbPath);
                await _db.CreateTableAsync<LocationEntry>();
            }
        }

        public async Task CloseAsync()
        {
            _db = null;
            await Task.CompletedTask;
        }

        public async Task ReopenAsync()
        {
            await CloseAsync();
            await OpenAsync();
        }

        private async Task EnsureDbAsync()
        {
            if (_db == null)
                await OpenAsync();
        }

        public async Task SaveEntryAsync(LocationEntry entry)
        {
            await EnsureDbAsync();

            if (string.IsNullOrWhiteSpace(entry.Latitude) || string.IsNullOrWhiteSpace(entry.Longitude))
            {
                entry.Latitude = "40.51.33 N";
                entry.Longitude = "2.12.32 O";
                entry.Location = "Olmeda de Cobeta";
            }

            await _db.InsertAsync(entry);
        }

        public async Task<List<LocationEntry>> GetAllEntriesAsync()
        {
            await EnsureDbAsync();
            return await _db.Table<LocationEntry>().OrderByDescending(c => c.Id).ToListAsync();
        }
    }
}