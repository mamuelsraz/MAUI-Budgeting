
using CommunityToolkit.Maui.Converters;
using MAUIBUDGET.Models;
using SkiaSharp;
using System.Globalization;

namespace MAUIBUDGET.Views
{
    public class PeriodToStringConverter : BaseConverterOneWay<Period, string>
    {
        public override string DefaultConvertReturnValue { get; set; } = "ERR_DEFAULT";

        public override string ConvertFrom(Period value, CultureInfo? culture)
        {
            switch (value)
            {
                case Period.Unset:
                    return "ERR_UNSET";
                case Period.Day:
                    return "day";
                case Period.WorkDay:
                    return "work day";
                case Period.Week:
                    return "week";
                case Period.Month:
                    return "month";
                default:
                    return "ERR_DEFAULT";
            }
        }
    }

    public class ViewPeriodToStringConverter : BaseConverterOneWay<ViewPeriod, string>
    {
        public override string DefaultConvertReturnValue { get; set; } = "ERR_DEFAULT";

        public override string ConvertFrom(ViewPeriod value, CultureInfo? culture)
        {
            switch (value)
            {
                case ViewPeriod.Unset:
                    return "ERR_UNSET";
                case ViewPeriod.Last7Days:
                    return "Spent in the last 7 days";
                case ViewPeriod.Last30Days:
                    return "Spent in the last 30 days";
                case ViewPeriod.ThisWeek:
                    return "Spent this week";
                case ViewPeriod.ThisMonth:
                    return "Spent this month";
                default:
                    return "ERR_DEFAULT";
            }
        }
    }

    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return parameter;

            return Binding.DoNothing;
        }
    }

    public class IntToColorConverter : IValueConverter
    {
        public Color DefaultColor { get; set; } = Colors.Red;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
                return Utils.GetCategoryColorFromInt(intValue);

            return DefaultColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not needed for one-way binding
            throw new NotImplementedException();
        }
    }
}
