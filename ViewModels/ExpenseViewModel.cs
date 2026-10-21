using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAUIBUDGET.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MAUIBUDGET.ViewModels
{
    public class ExpenseViewModel : ObservableObject
    {
        private int id;
        public int Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        private DateTime date;

        /// <summary>
        /// In Local Time
        /// </summary>
        public DateTime Date
        {
            get => date;
            set => SetProperty(ref date, value);
        }

        private double cost;
        public double Cost
        {
            get => cost;
            set => SetProperty(ref cost, value);
        }

        private string description;
        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        private int categoryId;
        public int CategoryId
        {
            get => categoryId;
            set => SetProperty(ref categoryId, value);
        }

        private bool isAlternate;
        public bool IsAlternate
        {
            get => isAlternate;
            set => SetProperty(ref isAlternate, value);
        }

        public ICommand EditExpenseCommand { get; }

        public ExpenseViewModel()
        {
            description = "";
            EditExpenseCommand = new AsyncRelayCommand(EditExpense);
        }

        public ExpenseViewModel(Expense expense)
        {
            description = expense.Description;

            Id = expense.Id;
            Date = expense.Date.ToLocalTime();
            Cost = expense.Cost;
            Description = expense.Description;
            CategoryId = expense.CategoryId;

            EditExpenseCommand = new AsyncRelayCommand(EditExpense);
        }

        public async Task EditExpense()
        {
            await Shell.Current.GoToAsync($"editexpense?id={this.Id}");
        }
    }
}