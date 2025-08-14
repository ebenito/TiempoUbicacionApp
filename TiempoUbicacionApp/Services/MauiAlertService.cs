using MudBlazor;
using MudBlazor.Utilities;
using System.Diagnostics;

namespace TiempoUbicacionApp.Services
{
    public class MauiAlertService : IAlertService
    {
        private ISnackbar? _snackbar;
        private readonly IFeedbackService _feedbackService;

        public MauiAlertService(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        public void Initialize(ISnackbar snackbar)
        {
            _snackbar = snackbar;
            try
            {
                _snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
                _snackbar.Configuration.SnackbarVariant = Variant.Filled;
                _snackbar.Configuration.VisibleStateDuration = 3000;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Inicializando snackbar] {ex?.Message}");
            }
        }

        public Task ShowToastAsync(string message)
        {
            if (_snackbar != null)
            {
                _snackbar.Add(message, Severity.Info);
            }
            else
            {
                return ShowDisplayAlertAsync(message);
            }
            return Task.CompletedTask;
        }

       
    public Task ShowLongToastAsync(string message)
    {
        // Dispara el feedback de error ANTES de mostrar el mensaje
        if (_feedbackService != null)
            Task.Run(async () => await _feedbackService.PlayErrorFeedbackAsync());

        if (_snackbar is null)
            return ShowDisplayAlertAsync(message);

        _snackbar.Add(
            // RenderFragment con estilo inline => texto negro
            // Para controlar el color del texto por snackbar, hay que usar un RenderFragment (contenido HTML)
            // y dale estilo inline.Así evitamos peleas de CSS y la prioridad de los estilos de MudBlazor.
            builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "style", "color:#000;font-weight:500;");
                builder.AddContent(2, message);
                builder.CloseElement();
            },
            Severity.Warning,
            cfg =>
            {
                cfg.VisibleStateDuration = 6000;
                cfg.SnackbarVariant = Variant.Filled;
                cfg.ShowCloseIcon = true;
            });



        return Task.CompletedTask;
    }


    private async Task ShowDisplayAlertAsync(string message)
        {
            await Application.Current.MainPage.DisplayAlert("Tiempo / Ubicación", message, "OK");
        }
    }
}
