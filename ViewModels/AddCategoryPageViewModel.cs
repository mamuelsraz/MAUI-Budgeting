using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAUIBUDGET.Models;
using MAUIBUDGET.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

namespace MAUIBUDGET.ViewModels
{
    public class AddCategoryPageViewModel : ObservableObject
    {
        private CategoryViewModel currentCategory;
        public CategoryViewModel CurrentCategory
        {
            get => currentCategory;
            set
            {
                var old = currentCategory;
                if (!SetProperty(ref currentCategory, value))
                    return;

                if (old != null)
                    old.PropertyChanged -= OnCurrentCategoryPropertyChanged;
                currentCategory.PropertyChanged += OnCurrentCategoryPropertyChanged;
                NotifyConfirmCommands();
            }
        }

        public IEnumerable<Period> PeriodValues { get; private set; }

        public ICommand CancelCategoryCreationCommand { get; }
        private readonly AsyncRelayCommand confirmCategoryCreationCommand;
        public ICommand ConfirmCategoryCreationCommand => confirmCategoryCreationCommand;
        public ICommand DeleteCategoryCommand { get; }
        private readonly AsyncRelayCommand confirmCategoryEditCommand;
        public ICommand ConfirmCategoryEditCommand => confirmCategoryEditCommand;

        private ExpensesService expensesService;

        public AddCategoryPageViewModel(ExpensesService expensesService)
        {
            this.expensesService = expensesService;
            CancelCategoryCreationCommand = new AsyncRelayCommand(CancelCategoryCreation);
            confirmCategoryCreationCommand = new AsyncRelayCommand(ConfirmCategoryCreation, IsCategoryValid);
            DeleteCategoryCommand = new AsyncRelayCommand(DeleteCategory);
            confirmCategoryEditCommand = new AsyncRelayCommand(ConfirmCategoryEdit, IsCategoryValid);

            PeriodValues = [Period.Day, Period.WorkDay, Period.Week, Period.Month];

            currentCategory = new CategoryViewModel();
            currentCategory.PropertyChanged += OnCurrentCategoryPropertyChanged;
        }

        bool IsCategoryValid() => !CurrentCategory.HasBudget || (CurrentCategory.Budget > 0 && CurrentCategory.BudgetPeriod != Period.Unset);

        void NotifyConfirmCommands()
        {
            confirmCategoryCreationCommand.NotifyCanExecuteChanged();
            confirmCategoryEditCommand.NotifyCanExecuteChanged();
        }

        private void OnCurrentCategoryPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            NotifyConfirmCommands();
        }

        private void ResetCurrentCategory()
        {
            CurrentCategory = new CategoryViewModel();
        }

        private async Task CancelCategoryCreation()
        {
            ResetCurrentCategory();
            await Shell.Current.GoToAsync("..");
        }

        private async Task DeleteCategory()
        {
            expensesService.DeleteCategory(CurrentCategory.Id);

            ResetCurrentCategory();
            await Shell.Current.GoToAsync("..");

        }

        private async Task ConfirmCategoryEdit()
        {
            expensesService.EditCategory(CurrentCategory.Id, CurrentCategory);

            ResetCurrentCategory();
            await Shell.Current.GoToAsync("..");
        }

        private async Task ConfirmCategoryCreation()
        {
            if (CurrentCategory == null)
                throw new NullReferenceException("Category cannot be null");

            expensesService.AddCategory(currentCategory);
            ResetCurrentCategory();
            await Shell.Current.GoToAsync("..");
        }

        public void SetCurrentCategoryByID(int id, bool copy = true)
        {
            var category = expensesService.Categories.FirstOrDefault(x => x.Id == id)
                ?? throw new NullReferenceException("Category Id not valid");

            if (!copy)
                CurrentCategory = category;
            else CurrentCategory = new CategoryViewModel()
            {
                Id = category.Id,
                Name = category.Name,
                HasBudget = category.HasBudget,
                Budget = category.Budget,
                BudgetPeriod = category.BudgetPeriod
            };
        }
    }
}
