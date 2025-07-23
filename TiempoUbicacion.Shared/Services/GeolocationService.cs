using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiempoUbicacion.Shared.Services
{
    // Solo la interfaz permanece en el proyecto compartido
    public class GeolocationServiceStub : IGeolocationService
    {
        public Task<(string Name, string FormattedLatitude, string FormattedLongitude, double Lat, double Lng)> GetCurrentLocationAsync()
        {
            throw new NotImplementedException("Implementación de plataforma requerida");
        }
    }
}
