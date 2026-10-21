using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAUIBUDGET.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MAUIBUDGET.ViewModels
{
    public class ExpensePageViewModel : ObservableObject
    {
        private ExpenseViewModel currentExpense;
        public ExpenseViewModel CurrentExpense
        {
            get => currentExpense;
            set => SetProperty(ref currentExpense, value);
        }

        private CategoryViewModel? selectedCategory = null;
        public CategoryViewModel? SelectedCategory
        {
            get => selectedCategory;
            set
            {
                if (SetProperty(ref selectedCategory, value))
                {
                    confirmExpenseCreationCommand?.NotifyCanExecuteChanged();
                    updateExpenseCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<CategoryViewModel> Categories => expensesService.Categories;

        public ICommand CancelExpenseCreationCommand { get; }
        private readonly AsyncRelayCommand confirmExpenseCreationCommand;
        public ICommand ConfirmExpenseCreationCommand => confirmExpenseCreationCommand;

        public ICommand CancelExpenseEditCommand { get; }
        private readonly AsyncRelayCommand updateExpenseCommand;
        public ICommand UpdateExpenseCommand => updateExpenseCommand;
        public ICommand DeleteExpenseCommand { get; }

        public event Action? Leaving;

        private readonly ExpensesService expensesService;

        private TimeSpan? timeOfDay;

        public ExpensePageViewModel(ExpensesService expensesService)
        {
            this.expensesService = expensesService;
            currentExpense = new ExpenseViewModel();
            ResetCurrentExpense();
            CancelExpenseCreationCommand = new AsyncRelayCommand(CancelExpenseCreation);
            confirmExpenseCreationCommand = new AsyncRelayCommand(ConfirmExpenseCreation, () => SelectedCategory != null);
            CancelExpenseEditCommand = new AsyncRelayCommand(CancelExpenseEdit);
            updateExpenseCommand = new AsyncRelayCommand(UpdateExpense, () => SelectedCategory != null);
            DeleteExpenseCommand = new AsyncRelayCommand(DeleteExpense);
        }

        private void ResetCurrentExpense()
        {
            CurrentExpense = new ExpenseViewModel() { Description = "", Date = DateTime.Now, Cost = 0 };
            timeOfDay = null;
            SelectedCategory = null;
        }

        public void SetDate(DateTime date)
        {
            CurrentExpense.Date = date;
            timeOfDay = date.TimeOfDay;
        }

        private ExpenseViewModel WithTimeOfDay(ExpenseViewModel expense)
        {
            return new ExpenseViewModel()
            {
                Id = expense.Id,
                Description = expense.Description,
                Cost = expense.Cost,
                Date = expense.Date.Date + (timeOfDay ?? DateTime.Now.TimeOfDay),
            };
        }

        private async Task NavigateAway(string route)
        {
            Leaving?.Invoke();
            await Shell.Current.GoToAsync(route);
        }

        private async Task CancelExpenseCreation()
        {
            ResetCurrentExpense();
            await NavigateAway("//home");
        }

        private async Task ConfirmExpenseCreation()
        {
            if (SelectedCategory == null)
                return;

            expensesService.AddExpense(WithTimeOfDay(CurrentExpense), SelectedCategory);

            ResetCurrentExpense();
            await NavigateAway("//home");
        }

        public void SetCurrentExpenseByID(int id)
        {
            var expense = expensesService.GetExpense(id);
            if (expense == null)
            {
                _ = Shell.Current.GoToAsync("..");
                return;
            }

            CurrentExpense = new ExpenseViewModel(expense);
            timeOfDay = expense.Date.ToLocalTime().TimeOfDay;
            SelectedCategory = expensesService.Categories.FirstOrDefault(x => x.Id == expense.CategoryId);
        }

        private async Task CancelExpenseEdit()
        {
            await NavigateAway("..");
        }

        private async Task UpdateExpense()
        {
            if (SelectedCategory == null)
                return;

            expensesService.UpdateExpense(WithTimeOfDay(CurrentExpense), SelectedCategory);
            await NavigateAway("..");
        }

        private async Task DeleteExpense()
        {
            bool ok = await Shell.Current.DisplayAlertAsync("Delete expense", $"Delete \"{CurrentExpense.Description}\"?", "Delete", "Cancel");
            if (!ok)
                return;

            expensesService.DeleteExpense(CurrentExpense.Id);
            await NavigateAway("..");
        }
    }
}
