using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace TiempoUbicacionApp.Services
{
    public class NativeAlertService : IAlertService
    {
        private readonly IFeedbackService _feedbackService;
        public NativeAlertService(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        public async Task ShowToastAsync(string message)
        {
            var toast = Toast.Make(message);
            await toast.Show();
        }

        public Task ShowLongToastAsync(string message)
        {
            // Dispara el feedback de error ANTES de mostrar el mensaje
            if (_feedbackService != null)
                Task.Run(async () => await _feedbackService.PlayErrorFeedbackAsync());


            return Toast.Make(message, ToastDuration.Long).Show();
        }


    }
}
