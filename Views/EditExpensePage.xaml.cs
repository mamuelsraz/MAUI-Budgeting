using MAUIBUDGET.ViewModels;

namespace MAUIBUDGET;

public partial class EditExpensePage : ContentPage, IQueryAttributable
{
    readonly ExpensePageViewModel viewModel;

    public EditExpensePage(ExpensePageViewModel viewModel)
    {
        InitializeComponent();

        this.viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.Leaving += Editor.HideKeyboard;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) && idObj is string idStr)
        {
            if (int.TryParse(idStr, out int id))
            {
                viewModel.SetCurrentExpenseByID(id);
            }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Editor.HideKeyboard();
    }
}
