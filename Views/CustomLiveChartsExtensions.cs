using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;

namespace MAUIBUDGET.Views
{
    public static partial class CustomLiveChartsExtensions
    {
        public static LiveChartsSettings AddLiveChartsAppSettings(this LiveChartsSettings settings)
        {
            settings.UseDefaults();
            settings.LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
            return settings;
        }
    }

}
