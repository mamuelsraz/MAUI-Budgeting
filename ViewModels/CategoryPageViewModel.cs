using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        public CategoryPageViewModel(ExpensesService expensesService)
        {
            this.expensesService = expensesService;
            CreateNewCategoryCommand = new AsyncRelayCommand(CreateNewCategory);
        }

        public async Task CreateNewCategory()
        {
            await Shell.Current.GoToAsync("newcategory");
        }
    }
}
