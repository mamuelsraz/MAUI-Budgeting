using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MAUIBUDGET.Models;
using MAUIBUDGET.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MAUIBUDGET.ViewModels
{
    public class MainPageViewModel : ObservableObject
    {
        public ObservableCollection<CategoryStatsViewModel> CategoryStatsList { get; set; } = new();

        private ViewPeriod viewPeriod;
        public ViewPeriod ViewPeriod
        {
            get { return viewPeriod; }
            set { SetProperty(ref viewPeriod, value); UpdateViewPeriod(); }
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

        public MainPageViewModel(ExpensesService expensesService)
        {
            this.expensesService = expensesService;
            expensesService.Categories.CollectionChanged += OnCategoriesCollectionChanged;

            ViewPeriod = ViewPeriod.ThisWeek;

            ResetCategoryStats(expensesService.Categories);
        }

        void ResetCategoryStats(ObservableCollection<CategoryViewModel> categories)
        {
            foreach (var item in CategoryStatsList)
            {
                item.Dispose();
            }

            CategoryStatsList.Clear();
            foreach (var category in categories)
            {
                CategoryStatsList.Add(new CategoryStatsViewModel(expensesService, this, category));
            }
        }

        private void UpdateViewPeriod()
        {
            DateTime startDate = Utils.GetStartDateFromViewPeriod(ViewPeriod);
            DateTime endDate = Utils.GetEndDateFromViewPeriod(ViewPeriod);
            SpentOverall = expensesService.GetOverallSpendingInPeriod(startDate, endDate);

            foreach (var stats in CategoryStatsList)
            {
                stats.Update();
            }

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

        private void OnCategoriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Reset
            if (e.Action == NotifyCollectionChangedAction.Reset)
                ResetCategoryStats(expensesService.Categories);

            // Remove or Replace
            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
            {
                if (e.OldItems != null)
                {
                    foreach (CategoryViewModel category in e.OldItems)
                    {
                        var categoryStats = CategoryStatsList.FirstOrDefault(x => x.Category == category);

                        if (categoryStats != null)
                        {
                            CategoryStatsList.Remove(categoryStats);
                            categoryStats.Dispose();
                        }
                    }
                }
            }

            // Add or Replace
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                if (e.NewItems != null)
                {
                    foreach (CategoryViewModel category in e.NewItems)
                    {
                        CategoryStatsList.Add(new CategoryStatsViewModel(expensesService, this, category));
                    }
                }
            }

            UpdateViewPeriod();
        }
    }
}
