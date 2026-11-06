namespace TiempoUbicacionApp.Helpers
{
    /// <summary>
    /// Acceso centralizado a los diálogos nativos de MAUI.
    /// Sustituye a <c>Application.Current.MainPage.DisplayAlert</c>, obsoleto desde MAUI 10:
    /// la página actual se obtiene de la ventana activa.
    /// </summary>
    public static class DialogHelper
    {
        /// <summary>Página visible de la primera ventana, o null si la app aún no tiene ventana.</summary>
        public static Page? CurrentPage =>
            Application.Current?.Windows.FirstOrDefault()?.Page;

        /// <summary>Muestra un aviso con un solo botón.</summary>
        public static Task ShowAlertAsync(string title, string message, string cancel = "OK")
        {
            var page = CurrentPage;
            return page is null
                ? Task.CompletedTask
                : page.DisplayAlertAsync(title, message, cancel);
        }

        /// <summary>Muestra una confirmación. Devuelve false si no hay página donde mostrarla.</summary>
        public static Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel)
        {
            var page = CurrentPage;
            return page is null
                ? Task.FromResult(false)
                : page.DisplayAlertAsync(title, message, accept, cancel);
        }
    }
}
