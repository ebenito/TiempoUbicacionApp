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
