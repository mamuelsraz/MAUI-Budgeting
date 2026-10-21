using MAUIBUDGET.ViewModels;

namespace MAUIBUDGET;

public partial class AddExpensePage : ContentPage, IQueryAttributable
{
    ExpensePageViewModel viewModel;

    public AddExpensePage(ExpensePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel; // To be able to inject services
        this.viewModel = viewModel;
        viewModel.Leaving += Editor.HideKeyboard;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Editor.HideKeyboard();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("description", out var descObj) && descObj is string desc)
            viewModel.CurrentExpense.Description = desc;

        if (query.TryGetValue("cost", out var costObj) && costObj is double cost)
            viewModel.CurrentExpense.Cost = cost;

        if (query.TryGetValue("date", out var dateObj) && dateObj is DateTime date)
            viewModel.SetDate(date);
    }
}
