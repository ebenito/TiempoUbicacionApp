using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiempoUbicacionApp.Models
{
    public class LocationEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string fechaActual { get; set; }
        public string horaLocal { get; set; }
        public string horaUtc { get; set; }
        public string desfase { get; set; }
        public string tipoHorario { get; set; }
        public string Location { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }

}
