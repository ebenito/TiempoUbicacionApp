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
        private readonly SQLiteAsyncConnection _db;

        public LocationDatabaseService()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "Ubicaciones.db");
            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<LocationEntry>().Wait();
        }

        public async Task InitAsync()
        {
            await Task.CompletedTask;
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
