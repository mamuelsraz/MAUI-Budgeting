using MAUIBUDGET.ViewModels;

namespace MAUIBUDGET;

public partial class EditCategoryPage : ContentPage, IQueryAttributable
{
    AddCategoryPageViewModel viewModel;

    public EditCategoryPage(AddCategoryPageViewModel viewModel)
    {
        InitializeComponent();

        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) && idObj is string idStr)
        {
            if (int.TryParse(idStr, out int id))
            {
                viewModel.SetCurrentCategoryByID(id);
            }
        }
    }
}