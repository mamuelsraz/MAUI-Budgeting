using MAUIBUDGET.Models;
using MAUIBUDGET.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace MAUIBUDGET.Platforms.Android
{
    public class AndroidNotificationManager : INotificationManager
    {
        public event EventHandler<AppNotification> NotificationReceived;
        public event EventHandler<AppNotification> NotificationRemoved;

        public AndroidNotificationManager()
        {
            // Subscribe to the static events we created earlier in NotificationListener
            NotificationListener.NotificationReceived += OnAndroidNotificationReceived;
            NotificationListener.NotificationRemoved += OnAndroidNotificationRemoved;
        }

        public bool IsPermissionGranted()
        {
            return PermissionHelper.IsNotificationServiceEnabled();
        }

        public void RequestPermission()
        {
            PermissionHelper.RequestNotificationPermission();
        }

        private void OnAndroidNotificationReceived(object sender, global::Android.Service.Notification.StatusBarNotification sbn)
        {
            var data = ConvertToAppNotification(sbn);
            NotificationReceived?.Invoke(this, data);
        }

        private void OnAndroidNotificationRemoved(object sender, global::Android.Service.Notification.StatusBarNotification sbn)
        {
            var data = ConvertToAppNotification(sbn);
            NotificationRemoved?.Invoke(this, data);
        }

        // Helper to extract clean data from the messy Android object
        private AppNotification ConvertToAppNotification(global::Android.Service.Notification.StatusBarNotification sbn)
        {
            string title = "";
            string text = "";
            string package = sbn.PackageName;
            try
            {
                var extras = sbn.Notification.Extras;
                title = extras?.GetString(global::Android.App.Notification.ExtraTitle) ?? "";
                text = extras?.GetString(global::Android.App.Notification.ExtraText) ?? "";
            }
            catch (Exception)
            {
                // Handle parsing errors gracefully
            }

            return new AppNotification
            {
                Title = title,
                Description = text,
                Timestamp = DateTime.Now,
                PackageName = package
            };
        }
    }
}
