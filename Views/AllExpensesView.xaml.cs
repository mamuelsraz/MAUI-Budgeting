using MAUIBUDGET.ViewModels;
using System.Collections.ObjectModel;

namespace MAUIBUDGET;

public partial class AllExpensesView : ContentView
{
    public static readonly BindableProperty CategoryProperty =
        BindableProperty.Create(
            nameof(Category),
            typeof(CategoryViewModel),
            typeof(AllExpensesView));

    public CategoryViewModel Category
    {
        get => (CategoryViewModel)GetValue(CategoryProperty);
        set => SetValue(CategoryProperty, value);
    }

    public static readonly BindableProperty ExpensesProperty =
        BindableProperty.Create(
            nameof(Expenses),
            typeof(ObservableCollection<ExpenseViewModel>),
            typeof(AllExpensesView));

    public ObservableCollection<ExpenseViewModel> Expenses
    {
        get => (ObservableCollection<ExpenseViewModel>)GetValue(ExpensesProperty);
        set => SetValue(ExpensesProperty, value);
    }

    public AllExpensesView()
    {
        InitializeComponent();
    }
}