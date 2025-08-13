using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiempoUbicacionApp.Services;

namespace TiempoUbicacionApp.Platforms.Android
{
    public class AndroidFeedbackService : IFeedbackService
    {
        public Task PlaySuccessFeedbackAsync()
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
            return Task.CompletedTask;
        }

        public Task PlayErrorFeedbackAsync()
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
            return Task.CompletedTask;
        }
    }

}
