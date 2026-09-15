using MAUIBUDGET.Models;
using MAUIBUDGET.Services;
using MAUIBUDGET.ViewModels;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using Plugin.LocalNotification.EventArgs;
using System.Diagnostics;

namespace MAUIBUDGET
{
    public partial class App : Application
    {
        static string walletPackageName = "com.google.android.apps.walletnfcrel";

        private ExpensesService expensesService;
        private TaskCompletionSource<Shell> shellReady = CreateShellReadySource();

        public App(INotificationManager notificationManager, ExpensesService expensesService)
        {
            InitializeComponent();

            this.expensesService = expensesService;
            notificationManager.NotificationReceived += OnNotificationReceived;
            LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;
        }

        private static TaskCompletionSource<Shell> CreateShellReadySource() =>
            new TaskCompletionSource<Shell>(TaskCreationOptions.RunContinuationsAsynchronously);

        private void OnNotificationReceived(object sender, AppNotification incomingNotification)
        {
            if (incomingNotification.PackageName != walletPackageName)
            {
                Debug.WriteLine("Notification not from the wallet, returning!");
                return;
            }

            double? cost = AmountParser.Parse(incomingNotification.Description);
            if (cost == null)
            {
                Debug.WriteLine("No amount found in the wallet notification, skipping: " + incomingNotification.Description);
                return;
            }

            ExpenseViewModel expense = new ExpenseViewModel()
            {
                Date = incomingNotification.Timestamp,
                Description = incomingNotification.Title,
                Cost = cost.Value
            };

            int id = expensesService.PushNotificationExpense(expense);

            var notification = new NotificationRequest
            {
                NotificationId = id,
                Title = expense.Description,
                Description = $"{expense.Cost} CZK",
                CategoryType = NotificationCategoryType.Service
            };

            LocalNotificationCenter.Current.Show(notification);
        }

        private async void OnNotificationActionTapped(NotificationActionEventArgs e)
        {
            if (e.IsDismissed)
            {
                expensesService.PopNotificationExpense(e.Request.NotificationId);
                return;
            }

            if (e.IsTapped)
            {
                await HandleCategoryOther(e.Request);
                return;
            }

            switch (e.ActionId)
            {
                case 200:
                    await HandleCategory(e.Request, 0);
                    break;

                case 300:
                    await HandleCategory(e.Request, 1);
                    break;

                case 400:
                    await HandleCategoryOther(e.Request);
                    break;
            }
        }

        private async Task HandleCategory(NotificationRequest request, int index)
        {
            var expense = expensesService.PopNotificationExpense(request.NotificationId);
            LocalNotificationCenter.Current.Cancel(request.NotificationId);

            if (expense != null)
            {
                var categories = expensesService.Categories.OrderBy(x => x.Id);

                var category = categories.ElementAtOrDefault(index);

                if (category == null)
                {
                    Debug.WriteLine($"Category {index} is null, returning");
                    return;
                }

                ExpenseViewModel expenseVM = new ExpenseViewModel(expense);
                expensesService.AddExpense(expenseVM, category);
            }
            else
                Debug.WriteLine($"Expense with id {request.NotificationId} not found in notif expenses.");
        }

        private async Task HandleCategoryOther(NotificationRequest request)
        {
            Expense? expense = expensesService.PopNotificationExpense(request.NotificationId);
            LocalNotificationCenter.Current.Cancel(request.NotificationId);

            if (expense == null)
            {
                Debug.WriteLine($"Expense with id {request.NotificationId} not found in notif expenses.");
                return;
            }

            var parameters = new ShellNavigationQueryParameters
            {
                { "description", expense.Description },
                { "cost", expense.Cost },
                { "date", expense.Date.ToLocalTime() }
            };

            Shell shell = await shellReady.Task;
            await MainThread.InvokeOnMainThreadAsync(() => shell.GoToAsync("//newexpense", parameters));
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            if (shellReady.Task.IsCompleted)
                shellReady = CreateShellReadySource();

            var ready = shellReady;
            var shell = new AppShell();
            shell.Loaded += (_, _) => ready.TrySetResult(shell);

            var window = new Window(shell);
            window.Destroying += (_, _) =>
            {
                if (shellReady == ready && ready.Task.IsCompleted)
                    shellReady = CreateShellReadySource();
            };

            return window;
        }
    }
}
