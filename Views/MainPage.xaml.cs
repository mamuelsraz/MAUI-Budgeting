using MAUIBUDGET.Models;
using MAUIBUDGET.Services;
using MAUIBUDGET.ViewModels;
using Plugin.LocalNotification;

namespace MAUIBUDGET
{
    public partial class MainPage : ContentPage
    {
        MainPageViewModel viewModel;
        INotificationManager notificationManager;

        public MainPage(MainPageViewModel viewModel, INotificationManager notificationManager)
        {
            InitializeComponent();

            this.viewModel = viewModel;

            this.notificationManager = notificationManager;
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Check if notifications are enabled
            if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
            {
                // Basic permission request
                await LocalNotificationCenter.Current.RequestNotificationPermission();
            }
            
            // Check permission using the interface
            if (!notificationManager.IsPermissionGranted())
            {
                bool answer = await DisplayAlertAsync(
                    "Permission Required",
                    "We need to see notifications to track budget. Open Settings?",
                    "Yes", "No");
                if (answer)
                {
                    notificationManager.RequestPermission();
                }
            }
        }
    }
}
