using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MAUIBUDGET.Models;
using MAUIBUDGET.Services;
using System.Collections.ObjectModel;

namespace MAUIBUDGET.ViewModels
{
    public class MainPageViewModel : ObservableObject
    {
        public ObservableCollection<CategoryStatsViewModel> CategoryStatsList { get; set; } = new();

        private ViewPeriod viewPeriod;
        public ViewPeriod ViewPeriod
        {
            get { return viewPeriod; }
            set { if (SetProperty(ref viewPeriod, value) && attached) RefreshAll(); }
        }

        private double spentOverall;
        public double SpentOverall
        {
            get => spentOverall;
            set => SetProperty(ref spentOverall, value);
        }

        public ObservableCollection<String> GraphLabels { get; set; } = new();
        public ObservableCollection<SolidColorPaint> GraphPaints { get; set; } = new();
        public ObservableCollection<ICartesianSeries> CategoriesSeries { get; set; } = new();

        private readonly ExpensesService expensesService;

        bool attached;

        public MainPageViewModel(ExpensesService expensesService)
        {
            this.expensesService = expensesService;
            viewPeriod = ViewPeriod.ThisWeek;
        }

        public void Attach()
        {
            if (attached) return;
            attached = true;
            expensesService.Changed += OnDataChanged;
            SyncStats();
            RefreshAll();
        }

        public void Detach()
        {
            if (!attached) return;
            attached = false;
            expensesService.Changed -= OnDataChanged;
            CategoryStatsList.Clear();
        }

        HashSet<int> SyncStats()
        {
            var categoryIds = expensesService.Categories.Select(x => x.Id).ToHashSet();
            foreach (var stats in CategoryStatsList.Where(x => !categoryIds.Contains(x.Category.Id)).ToList())
            {
                CategoryStatsList.Remove(stats);
            }

            HashSet<int> created = new();
            for (int i = 0; i < expensesService.Categories.Count; i++)
            {
                var category = expensesService.Categories[i];
                if (CategoryStatsList.Any(x => x.Category.Id == category.Id))
                    continue;

                CategoryStatsList.Insert(Math.Min(i, CategoryStatsList.Count), new CategoryStatsViewModel(expensesService, this, category));
                created.Add(category.Id);
            }

            return created;
        }

        private void OnDataChanged(int categoryId)
        {
            var created = SyncStats();

            if (!created.Contains(categoryId))
                CategoryStatsList.FirstOrDefault(x => x.Category.Id == categoryId)?.Update();

            RefreshOverall();
            RebuildBarChart();
        }

        private void RefreshAll()
        {
            RefreshOverall();

            foreach (var stats in CategoryStatsList)
            {
                stats.Update();
            }

            RebuildBarChart();
        }

        private void RefreshOverall()
        {
            DateTime startDate = Utils.GetStartDateFromViewPeriod(ViewPeriod);
            DateTime endDate = Utils.GetEndDateFromViewPeriod(ViewPeriod);
            SpentOverall = expensesService.GetOverallSpendingInPeriod(startDate, endDate);
        }

        private void RebuildBarChart()
        {
            CategoriesSeries.Clear();
            GraphLabels.Clear();
            GraphPaints.Clear();

            List<double> expectedValues = new();
            List<double> actualValues = new();

            foreach (var category in CategoryStatsList)
            {
                expectedValues.Add(category.Budget);
                actualValues.Add(category.SpentOverall);
                GraphLabels.Add(category.Category.Name);
                GraphPaints.Add(new SolidColorPaint(Utils.ColorToSKColor(Utils.GetCategoryColorFromInt(category.Category.Id))));
            }

            var series = new ColumnSeries<double>
            {
                Name = "Spent",
                Values = actualValues,
                IgnoresBarPosition = true,
                Stroke = null,
                Fill = new SolidColorPaint(SkiaSharp.SKColors.Red),
                MaxBarWidth = double.MaxValue,
            };
            series.PointMeasured += Series_PointMeasured;

            CategoriesSeries.Add(series);

            CategoriesSeries.Add(
                new ColumnSeries<double>
                {
                    Name = "Expected",
                    Values = expectedValues,
                    IgnoresBarPosition = true,
                    Stroke = new SolidColorPaint(SkiaSharp.SKColors.Gray, 2),
                    Fill = null,
                    MaxBarWidth = double.MaxValue,
                }
            );
        }

        private void Series_PointMeasured(ChartPoint<double, LiveChartsCore.SkiaSharpView.Drawing.Geometries.RoundedRectangleGeometry, LiveChartsCore.SkiaSharpView.Drawing.Geometries.LabelGeometry> point)
        {
            if (point.Context.Visual is null) return;

            point.Context.Visual.Fill = GraphPaints[point.Index % GraphPaints.Count];
        }
    }
}
