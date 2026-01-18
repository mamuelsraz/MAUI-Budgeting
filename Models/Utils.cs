using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace MAUIBUDGET.Models
{
    public static class Utils
    {
        public static double GetBudgetFactorForPeriod(Period period)
        {
            switch (period)
            {
                case Period.Day:
                    return 1d;

                case Period.WorkDay:
                    return 1d / (5d / 7d);

                case Period.Week:
                    return 1d / 7d;

                case Period.Month:
                    return 1d / 30.436875d;

                case Period.Unset:
                default:
                    return -1d;
            }
        }

        public static DateTime GetStartDateFromViewPeriod(ViewPeriod viewPeriod)
        {
            DateTime now = DateTime.Now;

            switch (viewPeriod)
            {
                default:
                case ViewPeriod.ThisWeek:
                    int daysSinceMonday = now.DayOfWeek == DayOfWeek.Sunday ? 6 : now.DayOfWeek - DayOfWeek.Monday;
                    return now.Date.AddDays(-daysSinceMonday);

                case ViewPeriod.ThisMonth:
                    return new DateTime(now.Year, now.Month, 1);

                case ViewPeriod.Last7Days:
                    return now.Date.AddDays(-6);

                case ViewPeriod.Last30Days:
                    return now.Date.AddDays(-30);
            }
        }

        public static DateTime GetEndDateFromViewPeriod(ViewPeriod viewPeriod)
        {
            DateTime now = DateTime.Now;

            switch (viewPeriod)
            {
                default:
                case ViewPeriod.ThisWeek:
                    int delta = DayOfWeek.Monday - now.DayOfWeek;

                    if (delta <= 0)
                        delta += 7;

                    return now.Date.AddDays(delta);

                case ViewPeriod.ThisMonth:
                    DateTime firstDayThisMonth = new DateTime(now.Year, now.Month, 1);
                    DateTime firstDayNextMonth = firstDayThisMonth.AddMonths(1);
                    return firstDayNextMonth;

                case ViewPeriod.Last7Days:
                    return now.Date.AddDays(1);

                case ViewPeriod.Last30Days:
                    return now.Date.AddDays(1);
            }
        }

        public static Color GetCategoryColorFromInt(int value)
        {
            value = value % 5;

            var key = value switch
            {
                0 => "CategoryColor0",
                1 => "CategoryColor1",
                2 => "CategoryColor2",
                3 => "CategoryColor3",
                4 => "CategoryColor4",
                _ => "CategoryColor0",
            };
            if (Application.Current!.Resources.TryGetValue(key, out var colorObj) && colorObj is Color mauiColor)
                return mauiColor;
            else
                return Color.FromRgba("D600AA");
        }

        public static SKColor ColorToSKColor(Color color) {
            return new SKColor(
                (byte)(color.Red * 255),
                (byte)(color.Green * 255),
                (byte)(color.Blue * 255),
                (byte)(color.Alpha * 255)
            );
        }
    }
}
