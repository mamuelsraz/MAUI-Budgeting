using System;
using System.Collections.Generic;
using System.Text;

namespace MAUIBUDGET.Models
{
    public enum Period
    {
        Unset = -1,
        Day = 0,
        WorkDay = 2,
        Week = 4,
        Month = 6,
    }

    public enum ViewPeriod { 
        Unset = -1,
        ThisWeek = 0,
        ThisMonth = 2,
        Last7Days = 4,
        Last30Days = 6
    }
}
