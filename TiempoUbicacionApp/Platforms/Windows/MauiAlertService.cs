using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiempoUbicacionApp.Services;

namespace TiempoUbicacionApp.Platforms.Windows
{
    public class MauiAlertService : IAlertService
    {
        public async Task ShowToastAsync(string message)
        {
            await Application.Current.MainPage.DisplayAlert("Tiempo / Ubicación", message, "OK");
        }
    }
}
