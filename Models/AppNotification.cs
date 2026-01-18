using System;
using System.Collections.Generic;
using System.Text;

namespace MAUIBUDGET.Models
{
    public class AppNotification
    {
        public string PackageName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
