using SQLite;

namespace TiempoUbicacionApp.Models
{
    /// <summary>
    /// Modelo de entrada de historial guardada en SQLite.    ///
    /// </summary>
    public class LocationEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime Date { get; set; }

        [Column("fechaActual")]
        public string FechaActual { get; set; } = string.Empty;

        [Column("horaLocal")]
        public string HoraLocal { get; set; } = string.Empty;

        [Column("horaUtc")]
        public string HoraUtc { get; set; } = string.Empty;

        [Column("desfase")]
        public string Desfase { get; set; } = string.Empty;

        [Column("tipoHorario")]
        public string TipoHorario { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
        public int Zona { get; set; }
    }
}
