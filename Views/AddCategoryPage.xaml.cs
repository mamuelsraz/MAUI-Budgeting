using MAUIBUDGET.ViewModels;
using System.ComponentModel;

namespace MAUIBUDGET;

public partial class AddCategoryPage : ContentPage
{
    public AddCategoryPage(AddCategoryPageViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel; // To be able to inject services
    }
}