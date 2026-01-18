using CommunityToolkit.Mvvm.ComponentModel;
using MAUIBUDGET.Models;
using System;
using System.Collections.Generic;
using System.Text;

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

        public ExpenseViewModel()
        {
            description = "";
        }

        public ExpenseViewModel(Expense expense)
        {
            description = expense.Description;

            Id = expense.Id;
            Date = expense.Date.ToLocalTime();
            Cost = expense.Cost;
            Description = expense.Description;
        }
    }
}