using System;
using System.Collections.Generic;
using System.Text;

namespace MAUIBUDGET.Models
{
    public class Category
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; }
        public bool HasBudget { get; set; }
        public Period BudgetPeriod { get; set; }
        public double Budget { get; set; }
    }
}
