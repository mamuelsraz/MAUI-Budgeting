using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAUIBUDGET.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MAUIBUDGET.ViewModels
{
    public class AddExpensePageViewModel : ObservableObject
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
            set => SetProperty(ref selectedCategory, value);
        }

        public ObservableCollection<CategoryViewModel> Categories => expensesService.Categories;

        public ICommand CancelExpenseCreationCommand { get; }
        public ICommand ConfirmExpenseCreationCommand { get; }

        private readonly ExpensesService expensesService;

        public AddExpensePageViewModel(ExpensesService expensesService)
        {
            this.expensesService = expensesService;
            currentExpense = new ExpenseViewModel();
            ResetCurrentExpense();
            CancelExpenseCreationCommand = new AsyncRelayCommand(CancelExpenseCreation);
            ConfirmExpenseCreationCommand = new AsyncRelayCommand(ConfirmExpenseCreation);
        }

        private void ResetCurrentExpense()
        {
            CurrentExpense = new ExpenseViewModel() { Description = "", Date = DateTime.Now, Cost = 0 };
        }

        private async Task CancelExpenseCreation()
        {
            ResetCurrentExpense();
            await Shell.Current.GoToAsync("//home");
        }

        private async Task ConfirmExpenseCreation()
        {   
            if (selectedCategory == null)
                throw new NullReferenceException("Category cannot be null");

            expensesService.AddExpense(CurrentExpense, SelectedCategory!);
            
            ResetCurrentExpense();
            await Shell.Current.GoToAsync("//home");
        }
    }
}
