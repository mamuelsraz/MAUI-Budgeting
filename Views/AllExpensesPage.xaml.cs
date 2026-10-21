using MAUIBUDGET.ViewModels;
using MauiIcons.Core;

namespace MAUIBUDGET;

public partial class AllExpensesPage : ContentPage, IQueryAttributable
{
    readonly AllExpensesPageViewModel viewModel;

    public AllExpensesPage(AllExpensesPageViewModel viewModel)
    {
        InitializeComponent();

        _ = new MauiIcon();

        this.viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.PageChanged += OnPageChanged;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("categoryId", out var idObj) && idObj is string idStr)
        {
            if (int.TryParse(idStr, out int id))
            {
                viewModel.SetCategoryByID(id);
            }
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.Attach();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        viewModel.Detach();
    }

    void OnPageChanged()
    {
#if ANDROID
        if (ExpensesList.Handler?.PlatformView is AndroidX.RecyclerView.Widget.RecyclerView recyclerView)
        {
            recyclerView.ScrollToPosition(0);
            return;
        }
#endif
        if (viewModel.Expenses.Count > 0)
            ExpensesList.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
    }
}
