using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace TiempoUbicacionApp.Helpers
{
    public static class CoordinatesHelper
    {
        /// <summary>
        /// Intenta parsear una coordenada que puede ser:
        /// - Decimal: "40.859167" o "-2.196667"
        /// - DMS con símbolos: "41º 03' 34'' N" o "41°03'34\"N"
        /// - DMS con puntos: "40.51.33 N" (interpreta como 40°51'33")
        /// - DMS con separadores ":" "40:51:33 N"
        /// - Palabras en español (Norte/Sur/Este/Oeste)
        /// Devuelve true y el valor en grados decimales (positivo norte/este, negativo sur/oeste).
        /// </summary>
        public static bool TryParseCoordinate(string input, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Normalizar
            string s = input.Trim();

            // Reemplazar palabras españolas por abreviaturas anglo (N,S,E,W)
            s = Regex.Replace(s, @"\b(norte)\b", "N", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\b(sur)\b", "S", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\b(este)\b", "E", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\b(oeste)\b", "W", RegexOptions.IgnoreCase);

            // También aceptar abreviatura 'O' como Oeste → W
            s = Regex.Replace(s, @"\bO\b", "W", RegexOptions.IgnoreCase);

            // Si el string contiene muchos puntos (ej. "40.51.33") y no es un decimal típico,
            // probablemente las puntos son separadores DMS -> sustituir por espacio
            int dotCount = s.Count(c => c == '.');
            if (dotCount >= 2)
                s = s.Replace('.', ' ');

            // Reemplazar otros separadores por espacio
            s = s.Replace('°', ' ')
                 .Replace('º', ' ')
                 .Replace(':', ' ')
                 .Replace("''", " ")
                 .Replace("″", " ")
                 .Replace("′", " ")
                 .Replace("“", " ")
                 .Replace("”", " ")
                 .Replace("\"", " ")
                 .Replace("'", " ");

            // Detectar letra de dirección (N S E W)
            var dirMatch = Regex.Match(s, @"\b([NSEW])\b", RegexOptions.IgnoreCase);
            char dir = dirMatch.Success ? char.ToUpper(dirMatch.Value[0]) : '\0';

            // Eliminar letras (dejamos solo números y separadores)
            string numbersOnly = Regex.Replace(s, @"[A-Za-z]", " ");

            // Extraer números (grados, minutos, segundos o decimal)
            var matches = Regex.Matches(numbersOnly, @"-?\d+(\.\d+)?");
            var nums = matches.Select(m => double.Parse(m.Value, CultureInfo.InvariantCulture)).ToArray();

            if (nums.Length == 0)
            {
                // última oportunidad: quizá la cadena es "-2,196667" con coma decimal
                string commaNormalized = input.Replace(',', '.');
                if (double.TryParse(commaNormalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var decAlt))
                {
                    value = decAlt;
                    return true;
                }
                return false;
            }

            double result;
            if (nums.Length == 1)
            {
                // Es decimal (o tenemos sólo grados)
                result = nums[0];
            }
            else if (nums.Length == 2)
            {
                // grados, minutos
                double deg = nums[0];
                double min = nums[1];
                result = deg + (min / 60.0);
            }
            else
            {
                // grados, minutos, segundos
                double deg = nums[0];
                double min = nums[1];
                double sec = nums[2];
                // cálculo: deg + min/60 + sec/3600
                // ejemplo: 40° 51' 33'' => 40 + 51/60 + 33/3600
                result = deg + (min / 60.0) + (sec / 3600.0);
            }

            // Aplicar signo según dirección (S o W => negativo)
            if (dir == 'S' || dir == 'W')
            {
                if (result > 0) result = -result;
            }

            value = result;
            return true;
        }

        /// <summary>
        /// Construye URL de Google Maps usando lat/lon en decimal.
        /// Devuelve null si no puede parsear.
        /// </summary>
        public static string? GetGoogleMapsUrl(string latStr, string lonStr, int zoom = 15)
        {
            if (TryParseCoordinate(latStr, out var lat) && TryParseCoordinate(lonStr, out var lon))
            {
                // usar InvariantCulture para usar punto decimal
                var latS = lat.ToString("G6", CultureInfo.InvariantCulture);
                var lonS = lon.ToString("G6", CultureInfo.InvariantCulture);
                return $"https://www.google.com/maps?q={latS},{lonS}&z={zoom}";
            }
            return null;
        }
    }
}
