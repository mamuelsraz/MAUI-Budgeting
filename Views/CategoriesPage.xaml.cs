using MAUIBUDGET.ViewModels;
using MauiIcons.Core;

namespace MAUIBUDGET
{
    public partial class CategoriesPage : ContentPage
    {
        public CategoriesPage(CategoryPageViewModel viewModel)
        {
            InitializeComponent();

            _ = new MauiIcon(); // Temporary Workaround for url styled namespace in xaml


            BindingContext = viewModel; // To be able to inject services
        }
    }
}
