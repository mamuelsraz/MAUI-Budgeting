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
            if (IsGroupSummaryOrOngoing(sbn))
                return;

            var data = ConvertToAppNotification(sbn);
            NotificationReceived?.Invoke(this, data);
        }

        private void OnAndroidNotificationRemoved(object sender, global::Android.Service.Notification.StatusBarNotification sbn)
        {
            var data = ConvertToAppNotification(sbn);
            NotificationRemoved?.Invoke(this, data);
        }

        private static bool IsGroupSummaryOrOngoing(global::Android.Service.Notification.StatusBarNotification sbn)
        {
            if (sbn.IsOngoing)
                return true;

            var flags = sbn.Notification?.Flags ?? 0;
            return flags.HasFlag(global::Android.App.NotificationFlags.GroupSummary);
        }

        private AppNotification ConvertToAppNotification(global::Android.Service.Notification.StatusBarNotification sbn)
        {
            string title = "";
            string text = "";
            try
            {
                var extras = sbn.Notification?.Extras;
                title = extras?.GetCharSequence(global::Android.App.Notification.ExtraTitle) ?? "";
                text = extras?.GetCharSequence(global::Android.App.Notification.ExtraText) ?? "";

                if (string.IsNullOrWhiteSpace(text))
                    text = extras?.GetCharSequence(global::Android.App.Notification.ExtraBigText) ?? "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Couldnt read notification extras: {ex.Message}");
            }

            return new AppNotification
            {
                Title = title,
                Description = text,
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(sbn.PostTime).LocalDateTime,
                PackageName = sbn.PackageName ?? ""
            };
        }
    }
}
