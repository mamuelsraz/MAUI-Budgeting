using Android.Content;
using Android.Provider;

namespace MAUIBUDGET.Platforms.Android
{
    public static class PermissionHelper
    {
        public static bool IsNotificationServiceEnabled()
        {
            var context = global::Android.App.Application.Context;
            string packageName = context.PackageName;

            string enabledListeners = Settings.Secure.GetString(context.ContentResolver, "enabled_notification_listeners");

            return !string.IsNullOrEmpty(enabledListeners) && enabledListeners.Contains(packageName);
        }

        public static void RequestNotificationPermission()
        {
            try
            {
                var intent = new Intent(Settings.ActionNotificationListenerSettings);
                intent.AddFlags(ActivityFlags.NewTask);

                global::Android.App.Application.Context.StartActivity(intent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening settings: {ex.Message}");
            }
        }
    }
}
