using CommunityToolkit.Maui.Alerts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiempoUbicacionApp.Services;

namespace TiempoUbicacionApp.Platforms.Android
{
    public class MauiAlertService : IAlertService
    {
        public async Task ShowToastAsync(string message)
        {
            var toast = Toast.Make(message);
            await toast.Show();
        }
    }
}
