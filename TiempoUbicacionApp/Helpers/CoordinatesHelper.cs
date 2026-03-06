using System.Globalization;
using System.Text.RegularExpressions;
using TiempoUbicacionApp.Services;

namespace TiempoUbicacionApp.Helpers
{
    /// <summary>
    /// Helper para construir URLs de mapas y parsear coordenadas.
    /// </summary>
    public static class CoordinatesHelper
    {
        /// <summary>
        /// Construye la URL del mapa según el proveedor configurado por el usuario.
        /// </summary>
        /// <param name="settingsService">Servicio de configuración inyectado desde el componente.</param>
        public static async Task<string?> GetMapUrlAsync(
            string latStr,
            string lonStr,
            ISettingsService settingsService,
            int zoom = 15,
            bool isEmbedded = true)
        {
            var provider = await settingsService.GetMapProviderAsync();

            return provider switch
            {
                MapProvider.Google => GetGoogleMapsUrl(latStr, lonStr, zoom, isEmbedded),
                MapProvider.OpenStreetMap => GetOSMEmbedUrl(latStr, lonStr, zoom),
                _ => GetOSMEmbedUrl(latStr, lonStr, zoom)
            };
        }

        /// <summary>
        /// Intenta parsear una coordenada en múltiples formatos:
        /// - Decimal:          "40.859167" / "-2.196667"
        /// - DMS con símbolos: "41º 03' 34'' N" / "41°03'34\"N"
        /// - DMS con puntos:   "40.51.33 N"  (interpreta como 40°51'33")
        /// - DMS con ":"       "40:51:33 N"
        /// - Palabras ES:      Norte / Sur / Este / Oeste
        /// </summary>
        public static bool TryParseCoordinate(string input, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            string s = input.Trim();

            // Normalizar palabras españolas a abreviaturas anglosajonas
            s = Regex.Replace(s, @"\b(norte)\b", "N", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\b(sur)\b", "S", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\b(este)\b", "E", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\b(oeste)\b", "W", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\bO\b", "W", RegexOptions.IgnoreCase);

            // Si hay múltiples puntos (ej. "40.51.33") son separadores DMS, no decimal
            int dotCount = s.Count(c => c == '.');
            if (dotCount >= 2)
                s = s.Replace('.', ' ');

            // Normalizar separadores a espacio
            s = s.Replace('°', ' ')
                 .Replace('º', ' ')
                 .Replace(':', ' ')
                 .Replace("''", " ")
                 .Replace("″", " ")
                 .Replace("′", " ")
                 .Replace("\u201c", " ")
                 .Replace("\u201d", " ")
                 .Replace("\"", " ")
                 .Replace("'", " ");

            // Detectar letra de dirección (N S E W)
            var dirMatch = Regex.Match(s, @"\b([NSEW])\b", RegexOptions.IgnoreCase);
            char dir = dirMatch.Success ? char.ToUpper(dirMatch.Value[0]) : '\0';

            // Extraer solo números
            string numbersOnly = Regex.Replace(s, @"[A-Za-z]", " ");
            var matches = Regex.Matches(numbersOnly, @"-?\d+(\.\d+)?");
            var nums = matches
                .Select(m => double.Parse(m.Value, CultureInfo.InvariantCulture))
                .ToArray();

            if (nums.Length == 0)
            {
                // Último intento: coma decimal ("−2,196667")
                string commaNormalized = input.Replace(',', '.');
                if (double.TryParse(commaNormalized, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var decAlt))
                {
                    value = decAlt;
                    return true;
                }
                return false;
            }

            double result = nums.Length switch
            {
                1 => nums[0],
                2 => nums[0] + nums[1] / 60.0,
                _ => nums[0] + nums[1] / 60.0 + nums[2] / 3600.0
            };

            // Sur u Oeste → negativo
            if ((dir == 'S' || dir == 'W') && result > 0)
                result = -result;

            value = result;
            return true;
        }

        /// <summary>
        /// Construye URL de Google Maps.
        /// </summary>
        public static string? GetGoogleMapsUrl(
            string latStr, string lonStr,
            int zoom = 15, bool isEmbebed = true)
        {
            if (!TryParseCoordinate(latStr, out var lat) ||
                !TryParseCoordinate(lonStr, out var lon))
                return null;

            var latS = lat.ToString("G", CultureInfo.InvariantCulture);
            var lonS = lon.ToString("G", CultureInfo.InvariantCulture);

            return isEmbebed
                ? $"https://maps.google.com/maps?q={latS},{lonS}&t=&z={zoom}&ie=UTF8&iwloc=&output=embed"
                : $"https://www.google.com/maps?q={latS},{lonS}&z={zoom}&ie=UTF8&iwloc=";
        }

        /// <summary>
        /// Construye URL de OpenStreetMap embed.
        /// </summary>
        public static string? GetOSMEmbedUrl(string latStr, string lonStr, int zoom = 15)
        {
            if (!TryParseCoordinate(latStr, out var lat) ||
                !TryParseCoordinate(lonStr, out var lon))
                return null;

            var latS = lat.ToString("G", CultureInfo.InvariantCulture);
            var lonS = lon.ToString("G", CultureInfo.InvariantCulture);

            double offset = 0.005;
            string bbox =
                $"{(lon - offset).ToString(CultureInfo.InvariantCulture)}%2C" +
                $"{(lat - offset).ToString(CultureInfo.InvariantCulture)}%2C" +
                $"{(lon + offset).ToString(CultureInfo.InvariantCulture)}%2C" +
                $"{(lat + offset).ToString(CultureInfo.InvariantCulture)}";

            return $"https://www.openstreetmap.org/export/embed.html?bbox={bbox}&layer=mapnik&marker={latS}%2C{lonS}";
        }
    }
}
