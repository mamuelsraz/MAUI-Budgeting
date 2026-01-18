using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MAUIBUDGET.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MAUIBUDGET.ViewModels
{
    public class CategoryPageViewModel : ObservableObject
    {
        public ObservableCollection<CategoryViewModel> Categories => expensesService.Categories;

        private readonly ExpensesService expensesService;

        public ICommand CreateNewCategoryCommand { get; }
        public ICommand ViewAllExpensesCommand { get; }

        public CategoryPageViewModel(ExpensesService expensesService)
        {
            this.expensesService = expensesService;
            CreateNewCategoryCommand = new AsyncRelayCommand(CreateNewCategory);
            ViewAllExpensesCommand = new AsyncRelayCommand<CategoryViewModel>(ViewAllExpenses);
        }

        public record ShowAllExpensesViewMessage;

        public async Task ViewAllExpenses(CategoryViewModel? category)
        {
            if (category== null)
            {
                throw new NullReferenceException("Edited Category cant be null");
            }

            WeakReferenceMessenger.Default.Send(new ShowAllExpensesViewMessage());

        }

        public async Task CreateNewCategory()
        {
            await Shell.Current.GoToAsync("newcategory");
        }
    }
}
