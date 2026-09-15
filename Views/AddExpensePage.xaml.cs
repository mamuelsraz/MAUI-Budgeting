using MAUIBUDGET.ViewModels;

namespace MAUIBUDGET;

public partial class AddExpensePage : ContentPage, IQueryAttributable
{
    AddExpensePageViewModel viewModel;

    public AddExpensePage(AddExpensePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel; // To be able to inject services
        this.viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("description", out var descObj) && descObj is string desc)
            viewModel.CurrentExpense.Description = desc;

        if (query.TryGetValue("cost", out var costObj) && costObj is double cost)
            viewModel.CurrentExpense.Cost = cost;

        if (query.TryGetValue("date", out var dateObj) && dateObj is DateTime date)
            viewModel.CurrentExpense.Date = date;
    }
}
