using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using MAUIBUDGET.Models;
using MAUIBUDGET.Services;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MAUIBUDGET.ViewModels
{
    public class CategoryStatsViewModel : ObservableObject, IDisposable
    {
        private ExpensesService expensesService;
        private MainPageViewModel mainPageViewModel;

        public List<DateTimePoint> ExpensePoints { get; set; } = new();
        public List<DateTimePoint> ExpectedPoints { get; set; } = new();
        public ObservableCollection<double> ExpenseSeparators { get; set; } = new();
        public ObservableCollection<double> DateSeparators { get; set; } = new();

        public object Sync { get; } = new object();

        private CategoryViewModel category;
        public CategoryViewModel Category
        {
            get => category;
            set => SetProperty(ref category, value);
        }

        private double spentOverall;
        public double SpentOverall
        {
            get => spentOverall;
            set => SetProperty(ref spentOverall, value);
        }

        private double budget;
        public double Budget
        {
            get => budget;
            set => SetProperty(ref budget, value);
        }

        private double spentPercent;
        public double SpentPercent
        {
            get => spentPercent;
            set => SetProperty(ref spentPercent, value);
        }

        public SolidColorPaint LinePaint { get; private set; }
        public SolidColorPaint PointPaint { get; private set; }
        public SolidColorPaint FillPaint { get; private set; }

        public CategoryStatsViewModel(ExpensesService expensesService, MainPageViewModel mainPageViewModel, CategoryViewModel category)
        {
            this.category = category;
            this.expensesService = expensesService;
            this.mainPageViewModel = mainPageViewModel;

            Category = category;
            Category.PropertyChanged += OnCategoryPropertyChanged;

            SKColor color = Utils.ColorToSKColor(Utils.GetCategoryColorFromInt(category.Id));

            LinePaint = new SolidColorPaint(color, 4);
            PointPaint = new SolidColorPaint(color, 6);
            FillPaint = new SolidColorPaint(color.WithAlpha(50), 2);

            ExpectedPoints.Add(new());
            ExpectedPoints.Add(new());

            GenerateStats();
        }

        public void Update()
        {
            GenerateStats();
        }

        private async Task UpdateGraphs(IEnumerable<Expense> dbExpenses, DateTime startDate, DateTime endDate, TimeSpan period)
        {
            OnPropertyChanging(nameof(ExpensePoints));
            lock (Sync)
            {
                ExpensePoints.Clear();
                ExpensePoints = new();
            }
            OnPropertyChanged(nameof(ExpensePoints));

            await Task.Factory.StartNew(() => Thread.Sleep(500)); // LiveCharts is buggy, temporary workaround
   
            ExpensePoints.Clear();
            ExpensePoints = new();
            OnPropertyChanged(nameof(ExpensePoints));

            OnPropertyChanging(nameof(ExpensePoints));
            OnPropertyChanging(nameof(ExpectedPoints));
            lock (Sync)
            {
                if (category.HasBudget)
                {
                    ExpectedPoints[0] = (new DateTimePoint(startDate, 0));
                    ExpectedPoints[1] = (new DateTimePoint(endDate, Budget));
                }

                double overallCost = 0;
                ExpensePoints.Add(new DateTimePoint(startDate, 0));
                foreach (var dbExpense in dbExpenses)
                {
                    overallCost += dbExpense.Cost;
                    ExpensePoints.Add(new DateTimePoint(dbExpense.Date.ToLocalTime(), overallCost));
                }
                ExpensePoints.Add(new DateTimePoint(DateTime.Now, overallCost));
            }
            OnPropertyChanged(nameof(ExpensePoints));
            OnPropertyChanged(nameof(ExpectedPoints));
        }

        private void GenerateStats()
        {
            DateTime startDate = Utils.GetStartDateFromViewPeriod(mainPageViewModel.ViewPeriod);
            DateTime endDate = Utils.GetEndDateFromViewPeriod(mainPageViewModel.ViewPeriod);
            TimeSpan period = endDate - startDate;
            var dbExpenses = expensesService.GetExpensesInCategory(Category.Id, startDate, endDate).OrderBy(x => x.Date);

            SpentOverall = Math.Round(dbExpenses.Sum(x => x.Cost));
            if (category.HasBudget)
            {
                Budget = Math.Round(Category.BudgetPerDay * period.TotalDays);
                SpentPercent = Math.Round(SpentOverall / Budget, 2);
            }
            else SpentPercent = -999f;

            _ = UpdateGraphs(dbExpenses, startDate, endDate, period);
        }

        private void OnCategoryPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            GenerateStats();
        }

        public Func<DateTime, string> DateFormatter { get; set; } =
            date => date.ToString("dd.MM");

        public void Dispose()
        {
            Category?.PropertyChanged -= OnCategoryPropertyChanged;
        }
    }
}
