using Android.App;
using Android.Service.Notification;
using Android.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace MAUIBUDGET.Platforms.Android
{
    [Service(
            Name = "com.companyname.mauibudget.NotificationListener",
            Label = "Notification Listener",
            Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE",
            Exported = true
        )]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    public class NotificationListener : NotificationListenerService
    {
        // We use a static event so we can subscribe to it from anywhere in the app easily
        public static event EventHandler<StatusBarNotification> NotificationReceived;
        public static event EventHandler<StatusBarNotification> NotificationRemoved;

        public override void OnCreate()
        {
            base.OnCreate();
            // Log to the Android Device Log (Logcat) to verify it starts
            Log.Info("MyListener", "Service Created");
        }

        // This fires when ANY app posts a notification
        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            if (sbn == null) return;

            // Log it so you can see it working in the Output window
            Log.Info("MyListener", $"Notification Received from: {sbn.PackageName}");

            // Trigger the event to tell the rest of your app
            NotificationReceived?.Invoke(this, sbn);
        }

        // This fires when a notification is swiped away or cleared
        public override void OnNotificationRemoved(StatusBarNotification sbn)
        {
            if (sbn == null) return;

            Log.Info("MyListener", $"Notification Removed: {sbn.PackageName}");
            NotificationRemoved?.Invoke(this, sbn);
        }

        // This confirms the service is actually connected to the Android system
        public override void OnListenerConnected()
        {
            Log.Info("MyListener", "Listener Connected!");
        }
    }
}
