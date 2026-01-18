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
        {
            viewModel.CurrentExpense.Description = desc;
        }
    }
}
