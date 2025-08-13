using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using TiempoUbicacionApp.Services;

namespace TiempoUbicacionApp.Platforms.Windows
{
    public class WindowsFeedbackService : IFeedbackService
    {
        public Task PlaySuccessFeedbackAsync()
        {
            SystemSounds.Asterisk.Play(); // Esto ahora debería funcionar correctamente
            return Task.CompletedTask;
        }

        public Task PlayErrorFeedbackAsync()
        {
            SystemSounds.Hand.Play(); // Esto también debería funcionar correctamente
            return Task.CompletedTask;
        }
    }

}
