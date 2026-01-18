using System;
using System.Collections.Generic;
using System.Text;

namespace MAUIBUDGET.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// IN UCT!!!!!
        /// </summary>
        public DateTime Date { get; set; }
        public double Cost { get; set; }
        public int CategoryId { get; set; }
    }
}
