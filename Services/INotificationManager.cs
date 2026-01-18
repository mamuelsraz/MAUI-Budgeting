using MAUIBUDGET.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MAUIBUDGET.Services
{
    public interface INotificationManager
    {
        event EventHandler<AppNotification> NotificationReceived;
        event EventHandler<AppNotification> NotificationRemoved;

        void RequestPermission();
        bool IsPermissionGranted();
    }
}
