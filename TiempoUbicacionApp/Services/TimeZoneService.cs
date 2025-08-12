using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiempoUbicacionApp.Services
{
    public class TimeZoneService : ITimeZoneService
    {
        public int GetStandardGMTZone()
        {
            // Zona horaria estándar (invierno)
            var january = new DateTime(DateTime.Now.Year, 1, 1);
            int offsetHours = (int)TimeZoneInfo.Local.GetUtcOffset(january).TotalHours;
            return offsetHours;
        }

        public bool IsDaylightSavingNow()
        {
            // Si ahora mismo estamos en horario de verano
            return TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now);
        }
    }
}
