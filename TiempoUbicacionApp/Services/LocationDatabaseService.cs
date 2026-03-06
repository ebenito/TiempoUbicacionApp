using SQLite;
using TiempoUbicacionApp.Models;

namespace TiempoUbicacionApp.Services
{
    /// <summary>
    /// Servicio de acceso a la base de datos SQLite.
    /// </summary>
    public class LocationDatabaseService
    {
        private readonly SQLiteAsyncConnection _db;
        private readonly string _dbPath;
        private bool _tableCreated = false;

        // IsOpen se mantiene por compatibilidad con el código de backup
        public bool IsOpen => true;

        public LocationDatabaseService()
        {
            _dbPath = Path.Combine(FileSystem.AppDataDirectory, "Ubicaciones.db");
            _db = new SQLiteAsyncConnection(_dbPath);
        }

        /// <summary>
        /// Crea la tabla si no existe. Se llama lazy en el primer uso y en OpenAsync.
        /// </summary>
        private async Task EnsureTableAsync()
        {
            if (_tableCreated) return;
            await _db.CreateTableAsync<LocationEntry>();
            _tableCreated = true;
        }

        // ── API pública ───────────────────────────────────────────────────

        /// <summary>
        /// Mantiene compatibilidad con el código que llama a OpenAsync().
        /// </summary>
        public async Task OpenAsync() => await EnsureTableAsync();
        public Task InitAsync() => OpenAsync();

        /// <summary>
        /// CloseAsync se mantiene para el flujo de restore (BackupService).
        /// Cierra y reabre la conexión interna.
        /// </summary>
        public async Task CloseAsync()
        {
            await _db.CloseAsync();
            _tableCreated = false;
        }

        public async Task ReopenAsync()
        {
            await CloseAsync();
            await OpenAsync();
        }

        public async Task SaveEntryAsync(LocationEntry entry)
        {
            await EnsureTableAsync();

            if (string.IsNullOrWhiteSpace(entry.Latitude) ||
                string.IsNullOrWhiteSpace(entry.Longitude))
            {
                entry.Latitude = "40.51.33 N";
                entry.Longitude = "2.12.32 O";
                entry.Location = "Olmeda de Cobeta";
            }

            await _db.InsertAsync(entry);
        }

        public async Task<List<LocationEntry>> GetAllEntriesAsync()
        {
            await EnsureTableAsync();
            return await _db.Table<LocationEntry>()
                            .OrderByDescending(c => c.Id)
                            .ToListAsync();
        }
    }
}
