using MAUIBUDGET.Models;
using MAUIBUDGET.ViewModels;

namespace MAUIBUDGET;

public partial class CategoryEditorView : ContentView
{
    public static readonly BindableProperty CategoryProperty =
        BindableProperty.Create(
            nameof(Category),
            typeof(CategoryViewModel),
            typeof(CategoryEditorView));

    public CategoryViewModel Category
    {
        get => (CategoryViewModel)GetValue(CategoryProperty);
        set => SetValue(CategoryProperty, value);
    }


    public static readonly BindableProperty PeriodValuesProperty =
    BindableProperty.Create(
        nameof(PeriodValues),
        typeof(IEnumerable<Period>),
        typeof(CategoryEditorView));

    public IEnumerable<Period> PeriodValues
    {
        get => (IEnumerable<Period>)GetValue(PeriodValuesProperty);
        set => SetValue(PeriodValuesProperty, value);
    }

    public CategoryEditorView()
	{
		InitializeComponent();

        PeriodValues = [Period.Day, Period.WorkDay, Period.Week, Period.Month];
    }
}