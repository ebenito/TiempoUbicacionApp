using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            _db = new SQLiteAsyncConnection(_dbPath);
            _db.CreateTableAsync<LocationEntry>().Wait();
        }

        public async Task CloseAsync()
        {
            _db = null;
            await Task.CompletedTask;
        }

        public async Task OpenAsync()
        {
            if (_db == null)
            {
                _db = new SQLiteAsyncConnection(_dbPath);
                await _db.CreateTableAsync<LocationEntry>();
            }
        }
        public async Task ReopenAsync()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "Ubicaciones.db");
            _db = new SQLiteAsyncConnection(dbPath);
            await _db.CreateTableAsync<LocationEntry>();
        }


        public async Task InitAsync()
        {
            await Task.CompletedTask;
        }

        public Task SaveEntryAsync(LocationEntry entry)
        {
            if (entry.Latitude == "" || entry.Longitude == "")
            {
                entry.Latitude = "40.51.33 N";
                entry.Longitude = "2.12.32 O";
                entry.Location = "Olmeda de Cobeta";
            }

            return _db.InsertAsync(entry);
        }

        public Task<List<LocationEntry>> GetAllEntriesAsync()
        {
            return _db.Table<LocationEntry>().OrderByDescending(c => c.Id).ToListAsync();
        }
    }

}
