using System;
using System.Drawing;
using Acr.UserDialogs;

namespace Mobile.Extensions
{
    public static class IUserDialogsExtensions
    {
        public static IDisposable ErrorToast(this IUserDialogs dialogs, string title, string message, TimeSpan duration)
        {
            return dialogs.Toast(new ToastConfig(message) { BackgroundColor = Color.Red, Duration = duration });
        }
    }
}
