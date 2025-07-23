using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiempoUbicacion.Shared.Models;

namespace TiempoUbicacion.Shared.Services
{
    public interface ILocationDatabaseService
    {
        Task InitAsync();
        Task SaveEntryAsync(LocationEntry entry);
        Task<List<LocationEntry>> GetAllEntriesAsync();
    }

    // Solo la interfaz permanece en el proyecto compartido
    public class LocationDatabaseServiceStub : ILocationDatabaseService
    {
        public Task InitAsync() => throw new NotImplementedException("Implementación de plataforma requerida");
        public Task SaveEntryAsync(LocationEntry entry) => throw new NotImplementedException("Implementación de plataforma requerida");
        public Task<List<LocationEntry>> GetAllEntriesAsync() => throw new NotImplementedException("Implementación de plataforma requerida");
    }
}
