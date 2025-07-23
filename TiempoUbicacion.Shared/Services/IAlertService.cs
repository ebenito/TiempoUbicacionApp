using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiempoUbicacion.Shared.Services
{
    public interface IAlertService
    {
        Task ShowToastAsync(string message);
    }
}
