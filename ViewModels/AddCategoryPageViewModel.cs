using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAUIBUDGET.Models;
using MAUIBUDGET.Services;
using System;
using System.Collections.Generic;
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
            set => SetProperty(ref currentCategory, value);
        }

        public IEnumerable<Period> PeriodValues { get; private set; }

        public ICommand CancelCategoryCreationCommand { get; }
        public ICommand ConfirmCategoryCreationCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand ConfirmCategoryEditCommand { get; }

        private ExpensesService expensesService;

        public AddCategoryPageViewModel(ExpensesService expensesService)
        {
            this.expensesService = expensesService;
            currentCategory = new CategoryViewModel();

            CancelCategoryCreationCommand = new AsyncRelayCommand(CancelCategoryCreation);
            ConfirmCategoryCreationCommand = new AsyncRelayCommand(ConfirmCategoryCreation);
            DeleteCategoryCommand = new AsyncRelayCommand(DeleteCategory);
            ConfirmCategoryEditCommand = new AsyncRelayCommand(ConfirmCategoryEdit);

            PeriodValues = [Period.Day, Period.WorkDay, Period.Week, Period.Month];
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
