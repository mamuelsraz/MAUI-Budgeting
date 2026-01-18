using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAUIBUDGET.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MAUIBUDGET.ViewModels
{
    public class CategoryViewModel : ObservableObject
    {
        private int id;
        public int Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private bool hasBudget = false;
        public bool HasBudget
        {
            get => hasBudget;
            set => SetProperty(ref hasBudget, value);
        }

        private Period budgetPeriod = Period.Unset;
        public Period BudgetPeriod
        {
            get => budgetPeriod;
            set => SetProperty(ref budgetPeriod, value);
        }

        private double budget = 0;
        public double Budget
        {
            get => budget;
            set => SetProperty(ref budget, value);
        }

        public double BudgetPerDay => Budget * Utils.GetBudgetFactorForPeriod(budgetPeriod);

        public ObservableCollection<ExpenseViewModel> LastExpenses { get; } = new ObservableCollection<ExpenseViewModel>();

        public ICommand EditCategoryCommand { get; }
        public ICommand ViewAllExpensesCommand { get; }

        public CategoryViewModel(Category dbCategory)
        {
            name = dbCategory.Name;

            Id = dbCategory.Id;
            Name = dbCategory.Name;
            HasBudget = dbCategory.HasBudget;
            BudgetPeriod = dbCategory.BudgetPeriod;
            Budget = dbCategory.Budget;

            EditCategoryCommand = new AsyncRelayCommand(EditCategory);
            ViewAllExpensesCommand = new AsyncRelayCommand(ViewAllExpenses);
        }

        public CategoryViewModel() {
            name = "";
            Id = -1;

            Name = "";
            HasBudget = false;
            BudgetPeriod = Period.Unset;
            Budget = 0;

            EditCategoryCommand = new AsyncRelayCommand(EditCategory);
            ViewAllExpensesCommand = new AsyncRelayCommand(ViewAllExpenses);
        }

        public async Task EditCategory()
        {
            await Shell.Current.GoToAsync($"editcategory?id={this.Id}");
        }

        public async Task ViewAllExpenses()
        {
            await Shell.Current.GoToAsync("expenses");
        }
    }
}