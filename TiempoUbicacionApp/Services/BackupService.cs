using System.Text;
using Microsoft.Maui.Storage;

namespace TiempoUbicacionApp.Services
{
    /// <summary>
    /// Centraliza toda la lógica de backup y restauración de la base de datos.
    /// Elimina la duplicación que existía en HistoryPage.razor y AndroidHistory.razor.
    /// </summary>
    public class BackupService
    {
        private readonly LocationDatabaseService _dbService;
        private readonly IAlertService _alertService;
        private const string DbFileName = "Ubicaciones.db";

        public BackupService(LocationDatabaseService dbService, IAlertService alertService)
        {
            _dbService = dbService;
            _alertService = alertService;
        }

        /// <summary>
        /// Comparte el archivo de base de datos usando el diálogo nativo del sistema.
        /// </summary>
        public async Task ShareDatabaseAsync()
        {
            try
            {
                var dbPath = GetDbPath();

                if (!File.Exists(dbPath))
                {
                    await _alertService.ShowToastAsync("No se encontró la base de datos.");
                    return;
                }

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Backup de la BD",
                    File = new ShareFile(dbPath)
                });
            }
            catch (Exception ex)
            {
                await _alertService.ShowLongToastAsync($"No se pudo compartir la BD: {ex.Message}");
            }
        }

        /// <summary>
        /// Permite al usuario seleccionar un archivo .db y restaurarlo como base de datos activa.
        /// Realiza validación de cabecera SQLite antes de sobrescribir.
        /// </summary>
        public async Task<bool> RestoreDatabaseAsync()
        {
            try
            {
                var sqliteFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.Android, new[] { "application/x-sqlite3", "application/octet-stream", ".db", ".sqlite", ".sqlite3" } },
                        { DevicePlatform.WinUI,   new[] { ".db", ".sqlite", ".sqlite3" } },
                        { DevicePlatform.macOS,   new[] { ".db", ".sqlite", ".sqlite3" } },
                        { DevicePlatform.iOS,     new[] { "public.database", ".db", ".sqlite", ".sqlite3" } }
                    });

                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecciona el archivo de backup (.db)",
                    FileTypes = sqliteFileType
                });

                if (result == null)
                    return false; // Usuario canceló

                // Copiar a temporal
                var tempFile = Path.Combine(FileSystem.CacheDirectory, $"restore_{Guid.NewGuid()}.db");
                using (var src = await result.OpenReadAsync())
                using (var dst = File.Create(tempFile))
                {
                    await src.CopyToAsync(dst);
                }

                // Validar cabecera SQLite
                if (!await IsValidSqliteFileAsync(tempFile))
                {
                    File.Delete(tempFile);
                    await _alertService.ShowToastAsync("El archivo no es una base de datos SQLite válida.");
                    return false;
                }

                var dbPath = GetDbPath();
                var backupPath = Path.Combine(
                    FileSystem.AppDataDirectory,
                    $"Ubicaciones_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");

                // Cerrar la BD si está abierta
                var wasOpen = _dbService.IsOpen;
                if (wasOpen)
                    await _dbService.CloseAsync();

                // Backup del fichero actual
                if (File.Exists(dbPath))
                    File.Copy(dbPath, backupPath, overwrite: true);

                // Restaurar
                File.Copy(tempFile, dbPath, overwrite: true);

                if (wasOpen)
                    await _dbService.OpenAsync();

                try { File.Delete(tempFile); } catch { /* ignorar */ }

                await _alertService.ShowToastAsync("✅ Base de datos restaurada correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                await _alertService.ShowLongToastAsync($"No se pudo restaurar la BD: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Envía la base de datos por email como adjunto.
        /// En Windows abre el cliente de correo sin adjunto (limitación de la plataforma).
        /// </summary>
        public async Task SendDatabaseByEmailAsync()
        {
            try
            {
                var dbPath = GetDbPath();

                if (!File.Exists(dbPath))
                {
                    await _alertService.ShowToastAsync("No se encontró la base de datos.");
                    return;
                }

                if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
                {
                    // Windows no soporta adjuntos via mailto, abrimos el cliente
                    var mailto = "mailto:?subject=Backup%20TiempoUbicacionApp&body=Adjunta%20la%20BD%20manualmente.";
                    await Launcher.Default.OpenAsync(mailto);
                }
                else
                {
                    var message = new EmailMessage
                    {
                        Subject = "Backup de BD TiempoUbicacionApp",
                        Body = "Adjunto el archivo de la base de datos."
                    };
                    message.Attachments.Add(new EmailAttachment(dbPath));
                    await Email.Default.ComposeAsync(message);
                }
            }
            catch (Exception ex)
            {
                await _alertService.ShowLongToastAsync($"No se pudo enviar la BD: {ex.Message}");
            }
        }

        // ── Helpers privados ─────────────────────────────────────────────────

        private static string GetDbPath() =>
            Path.Combine(FileSystem.AppDataDirectory, DbFileName);

        private static async Task<bool> IsValidSqliteFileAsync(string filePath)
        {
            try
            {
                using var fs = File.OpenRead(filePath);
                var header = new byte[16];
                var read = await fs.ReadAsync(header, 0, header.Length);
                var headerStr = Encoding.ASCII.GetString(header, 0, Math.Max(0, read));
                return headerStr.StartsWith("SQLite format 3");
            }
            catch
            {
                return false;
            }
        }
    }
}
