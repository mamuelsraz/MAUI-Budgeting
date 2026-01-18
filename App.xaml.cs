using MAUIBUDGET.Models;
using MAUIBUDGET.Services;
using MAUIBUDGET.ViewModels;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MAUIBUDGET
{
    public partial class App : Application
    {
        static string walletPackageName = "com.google.android.apps.walletnfcrel";
        static string regexMatch = @"\w*?(\d+\.?\d*).*";

        private ExpensesService expensesService;

        public App(INotificationManager notificationManager, ExpensesService expensesService)
        {
            InitializeComponent();

            this.expensesService = expensesService;
            notificationManager.NotificationReceived += OnNotificationReceived;
            LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;
        }

        private void OnNotificationReceived(object sender, AppNotification incomingNotification)
        {
            if (incomingNotification.PackageName != walletPackageName)
            {
                Debug.WriteLine("Notification not from the wallet, returning!");
                return;
            }

            //if (incomingNotification.PackageName == AppInfo.PackageName)
            //{
            //    Debug.WriteLine("Notification not from the wallet, returning!");
            //    return;
            //}

            ExpenseViewModel expense = new ExpenseViewModel()
            {
                Date = incomingNotification.Timestamp,
                Description = incomingNotification.Title,
                Cost = 0
            };

            Match m = Regex.Match(incomingNotification.Description, regexMatch);
            if (m.Success && m.Groups.Count >= 2)
            {
                string s = m.Groups[1].Value;
                Debug.WriteLine("Found string " + s);
                if (double.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, out var parsedCost))
                {
                    expense.Cost = parsedCost;
                }
                else
                {
                    Debug.WriteLine($"Couldnt parse string {s} into a double");
                }
            }
            else
            {
                Debug.WriteLine("$Couldnt find a match in the description string: " + incomingNotification.Description);
            }

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

            LocalNotificationCenter.Current.Cancel(request.NotificationId);
        }

        private async Task HandleCategoryOther(NotificationRequest request)
        {
            Expense? expense = expensesService.PopNotificationExpense(request.NotificationId);

            if (expense != null)
            {
                await Shell.Current.GoToAsync($"//newexpense?description={expense.Description}");
            }
            else
                Debug.WriteLine($"Expense with id {request.NotificationId} not found in notif expenses.");

            LocalNotificationCenter.Current.Cancel(request.NotificationId);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
