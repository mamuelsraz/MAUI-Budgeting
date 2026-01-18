using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.Messaging;
using MAUIBUDGET.ViewModels;
using MauiIcons.Core;
using Microsoft.Maui.Controls.Shapes;
using static MAUIBUDGET.ViewModels.CategoryPageViewModel;

namespace MAUIBUDGET
{
    public partial class CategoriesPage : ContentPage
    {
        public CategoriesPage(CategoryPageViewModel viewModel)
        {
            InitializeComponent();

            _ = new MauiIcon(); // Temporary Workaround for url styled namespace in xaml


            BindingContext = viewModel; // To be able to inject services

            WeakReferenceMessenger.Default.Register<ShowAllExpensesViewMessage>(this, async (_, _) => await ShowAllExpenses());
        }

        public async Task ShowAllExpenses()
        {
            var popup = new AllExpensesView();
            await this.ShowPopupAsync(popup, new PopupOptions
            {
                Shape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(4),
                    StrokeThickness = 1
                },
                Shadow = null
            });
        }
    }
}