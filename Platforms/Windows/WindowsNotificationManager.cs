using MAUIBUDGET.Models;
using MAUIBUDGET.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace MAUIBUDGET.Platforms.Windows
{
    internal class WindowsNotificationManager : INotificationManager
    {
        public event EventHandler<AppNotification> NotificationReceived;
        public event EventHandler<AppNotification> NotificationRemoved;

        public bool IsPermissionGranted()
        {
            return true;
        }

        public void RequestPermission()
        {
            
        }
    }
}
