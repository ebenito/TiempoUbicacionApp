using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace TiempoUbicacionApp.Services
{
    
    public interface IOneDriveService
    {
        Task<bool> EnsureSignInAsync(); // opcional, para forzar login antes

        public Task BackupDatabaseAsync(); 

        public Task RestoreDatabaseAsync();

        Task UploadBackupAsync(string localFilePath, string remoteFileName = "backup.db");

        Task DownloadBackupAsync(string localFilePath, string remoteFileName = "backup.db");

        bool IsSignedIn { get; }
    }

    /// <summary>
    /// Autenticación con MSAL (PublicClient) y llamadas REST a Microsoft Graph.
    /// Evita fricciones del SDK v5 y es válido para Windows y Android (nota Android más abajo).
    /// </summary>
    public class OneDriveService : IOneDriveService
    {
        private readonly string _clientId;
        private readonly string _tenantId;
        //private readonly string[] _scopes = new[] { "Files.ReadWrite.All", "User.Read", "offline_access" };
        private readonly string[] _scopes = new[] { "Files.ReadWrite", "offline_access" };


        private readonly IPublicClientApplication _pca;
        private readonly HttpClient _http;
        private IAccount? _account;

        public bool IsSignedIn => _account != null;

        // Carpeta de la app en OneDrive (bajo la raíz del usuario)
        private const string AppFolderName = "TiempoUbicacionApp";

        public OneDriveService(string clientId, string tenantId, HttpClient? httpClient = null)
        {
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _tenantId = tenantId ?? "common";

//            string redirectUri;

//#if ANDROID
//            redirectUri = $"msal{clientId}://auth";
//#elif WINDOWS
//            redirectUri = "http://localhost";
//#else
//            redirectUri = "http://localhost"; // por defecto
//#endif

            _pca = PublicClientApplicationBuilder
                .Create(_clientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, "common")
                .WithTenantId(_tenantId)
                .WithRedirectUri(DeviceInfo.Platform == DevicePlatform.Android
                    ? $"msauth://com.tubkala.tiempoubicacionapp/[aqui hash paquete]"
                    : "https://login.microsoftonline.com/common/oauth2/nativeclient")
                //.WithRedirectUri("msal[aqui ClientID]")
                .WithLogging((level, message, containsPii) =>
                {
                    System.Diagnostics.Debug.WriteLine($"MSAL [{level}] {message}");
                }, LogLevel.Verbose, enablePiiLogging: true, enableDefaultPlatformLogging: true)
                .Build();

            _http = httpClient ?? new HttpClient();
        }

        public async Task<bool> EnsureSignInAsync()
        {
            try
            {
                var accounts = await _pca.GetAccountsAsync();
                _account = accounts.FirstOrDefault();
                if (_account != null)
                {
                    // Intento silencioso
                    await _pca.AcquireTokenSilent(_scopes, _account).ExecuteAsync();
                    return true;
                }
            }
            catch { /* cae a interactivo */ }

            try
            {
                var result = await _pca.AcquireTokenInteractive(_scopes)
                    .WithUseEmbeddedWebView(false)      // usa navegador del sistema (Windows)
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync();

                _account = result.Account;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task UploadBackupAsync(string localFilePath, string remoteFileName = "backup.db")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(localFilePath))
                    throw new ArgumentNullException(nameof(localFilePath));
                if (!File.Exists(localFilePath))
                    throw new FileNotFoundException("Archivo local no encontrado", localFilePath);

                var token = await GetAccessTokenAsync();

                // Asegura que la carpeta exista
                await EnsureAppFolderAsync(token);

                // PUT /me/drive/root:/TiempoUbicacionApp/backup.db:/content
                //var uploadUrl = $"https://graph.microsoft.com/v1.0/me/drive/root:/{AppFolderName}/{remoteFileName}:/content";
                var uploadUrl = $"https://graph.microsoft.com/v1.0/me/drive/special/approot:/{remoteFileName}:/content";

                var tempPath = Path.Combine(Path.GetTempPath(), "Ubicaciones.db");
                File.Copy(localFilePath, tempPath, overwrite: true);
                using var stream = File.OpenRead(tempPath);

                //using var stream = File.OpenRead(localFilePath);
                using var content = new StreamContent(stream);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                using var req = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                req.Content = content;

                using var resp = await _http.SendAsync(req);
                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"Error subiendo backup: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{body}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error durante el backup de la base de datos.", ex);
            }
        }

        public async Task DownloadBackupAsync_BACK(string localFilePath, string remoteFileName = "backup.db")
        {
            if (string.IsNullOrWhiteSpace(localFilePath))
                throw new ArgumentNullException(nameof(localFilePath));

            var token = await GetAccessTokenAsync();

            // GET /me/drive/root:/TiempoUbicacionApp/backup.db:/content
            // var downloadUrl = $"https://graph.microsoft.com/v1.0/me/drive/root:/{AppFolderName}/{remoteFileName}:/content";
            var downloadUrl = $"https://graph.microsoft.com/v1.0/me/drive/special/approot:/{remoteFileName}:/content";

            using var req = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var resp = await _http.SendAsync(req);
            if (resp.StatusCode == HttpStatusCode.NotFound)throw new FileNotFoundException("No existe backup en OneDrive.", remoteFileName);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Error descargando backup: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{body}");
            }

            using var inStream = await resp.Content.ReadAsStreamAsync();
            using var outStream = File.Create(localFilePath);
            await inStream.CopyToAsync(outStream);
        }

        public async Task DownloadBackupAsync(string localDbPath, string remoteFileName = "backup.db")
        {
            if (string.IsNullOrWhiteSpace(localDbPath))
                throw new ArgumentNullException(nameof(localDbPath));

            var tempPath = Path.Combine(Path.GetTempPath(), $"restore_{Guid.NewGuid()}.db");

            // Instanciar el servicio de base de datos
            var dbService = new LocationDatabaseService();
            var dbWasOpen = dbService.IsOpen;

            try
            {
                // Cerrar la base de datos si está abierta
                if (dbWasOpen)
                    await dbService.CloseAsync();

                var token = await GetAccessTokenAsync();
                var downloadUrl = $"https://graph.microsoft.com/v1.0/me/drive/special/approot:/{remoteFileName}:/content";

                using var req = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var resp = await _http.SendAsync(req);
                if (resp.StatusCode == HttpStatusCode.NotFound)
                    throw new FileNotFoundException("No se encontró el archivo de backup en OneDrive.");

                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    throw new IOException($"Error al descargar el backup: {resp.StatusCode} - {body}");
                }

                using var inStream = await resp.Content.ReadAsStreamAsync();
                using (var outStream = File.Create(tempPath))
                {
                    await inStream.CopyToAsync(outStream);
                }

                File.Copy(tempPath, localDbPath, overwrite: true);


                // Reabrir la base de datos si estaba abierta
                if (dbWasOpen)
                    await dbService.OpenAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error durante la restauración de la base de datos.", ex);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { /* Ignorar errores de limpieza */ }
                }
            }
        }

        private async Task<AuthenticationResult> AcquireInteractiveAsync()
        {
#if ANDROID
            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            return await _pca.AcquireTokenInteractive(_scopes)
                .WithParentActivityOrWindow(activity)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync();
#else

            return await _pca.AcquireTokenInteractive(_scopes)
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync();
#endif
        }

        private async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var accounts = await _pca.GetAccountsAsync();
                var first = accounts.FirstOrDefault();
                if (first != null)
                {
                    var silent = await _pca.AcquireTokenSilent(_scopes, first).ExecuteAsync();
                    return silent.AccessToken;
                }
            }
            catch (MsalUiRequiredException)
            {
                // Necesita interacción
            }

            var interactive = await AcquireInteractiveAsync();
            return interactive.AccessToken;
        }

        //        private async Task<string> GetAccessTokenAsync()
        //        {
        //            AuthenticationResult result;

        //            try
        //            {
        //                var accounts = await _pca.GetAccountsAsync();
        //                _account = accounts.FirstOrDefault();
        //                if (_account != null)
        //                {
        //                    var silent = await _pca.AcquireTokenSilent(_scopes, _account).ExecuteAsync();
        //                    return silent.AccessToken;
        //                }
        //            }
        //            catch { /* interactivo abajo */ }


        //#if ANDROID
        //            var activity = Platform.CurrentActivity; // viene de Microsoft.Maui.ApplicationModel
        //            result = await _pca.AcquireTokenInteractive(_scopes)
        //                .WithParentActivityOrWindow(activity)
        //                .ExecuteAsync();
        //#else
        //            result = await _pca.AcquireTokenInteractive(_scopes)
        //                .WithUseEmbeddedWebView(false)
        //                .WithPrompt(Prompt.SelectAccount)
        //                .ExecuteAsync();
        //#endif


        //            _account = result.Account;
        //            return result.AccessToken;
        //        }

        private async Task EnsureAppFolderAsync(string token)
        {
            // ¿Existe /TiempoUbicacionApp?
            var checkUrl = $"https://graph.microsoft.com/v1.0/me/drive/special/approot:/";
            //var checkUrl = $"https://graph.microsoft.com/v1.0/me/drive/root:/{AppFolderName}";

            using (var checkReq = new HttpRequestMessage(HttpMethod.Get, checkUrl))
            {
                checkReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var checkResp = await _http.SendAsync(checkReq);
                if (checkResp.IsSuccessStatusCode) return; // ya existe

                if (checkResp.StatusCode != HttpStatusCode.NotFound)
                {
                    var body = await checkResp.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"Error comprobando carpeta: {(int)checkResp.StatusCode} {checkResp.ReasonPhrase}\n{body}");
                }
            }

            // Crear carpeta: POST /me/drive/root/children
            var createUrl = "https://graph.microsoft.com/v1.0/me/drive/root/children";
            var payload = new Dictionary<string, object>
            {
                { "name", AppFolderName },
                { "folder", new { } },
                { "@microsoft.graph.conflictBehavior", "fail" }
            };

            var json = JsonSerializer.Serialize(payload);
            using var createReq = new HttpRequestMessage(HttpMethod.Post, createUrl);
            createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            createReq.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var createResp = await _http.SendAsync(createReq);
            if (!createResp.IsSuccessStatusCode)
            {
                var body = await createResp.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Error creando carpeta: {(int)createResp.StatusCode} {createResp.ReasonPhrase}\n{body}");
            }
        }

        public async Task BackupDatabaseAsync()
        {
            try
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "Ubicaciones.db");

                if (!File.Exists(dbPath))
                    throw new FileNotFoundException("La base de datos local no existe.", dbPath);

                await UploadBackupAsync(dbPath, "backup.db");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al realizar el backup en OneDrive.", ex);
            }
        }

        public async Task RestoreDatabaseAsync()
        {
            try
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "Ubicaciones.db");

                await DownloadBackupAsync(dbPath, "backup.db");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al restaurar el backup desde OneDrive.", ex);
            }
        }



    }
}
