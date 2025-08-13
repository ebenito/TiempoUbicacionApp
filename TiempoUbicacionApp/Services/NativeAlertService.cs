using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace TiempoUbicacionApp.Services
{
    public class NativeAlertService : IAlertService
    {
        public async Task ShowToastAsync(string message)
        {
            var toast = Toast.Make(message);
            await toast.Show();
        }

        public Task ShowLongToastAsync(string message)
        {
            return Toast.Make(message, ToastDuration.Long).Show();
        }


    }
}
