using CommunityToolkit.Maui.Core.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using MAUIBUDGET.ViewModels;

namespace MAUIBUDGET;

public partial class ExpenseEditorView : ContentView
{
    public static readonly BindableProperty CategoryListProperty =
        BindableProperty.Create(
            nameof(CategoryList),
            typeof(IEnumerable<CategoryViewModel>),
            typeof(ExpenseEditorView));

    public IEnumerable<CategoryViewModel> CategoryList
    {
        get => (IEnumerable<CategoryViewModel>)GetValue(CategoryListProperty);
        set => SetValue(CategoryListProperty, value);
    }

    public static readonly BindableProperty SelectedCategoryProperty =
        BindableProperty.Create(
            nameof(SelectedCategory),
            typeof(CategoryViewModel),
            typeof(ExpenseEditorView));

    public CategoryViewModel SelectedCategory
    {
        get => (CategoryViewModel)GetValue(SelectedCategoryProperty);
        set => SetValue(SelectedCategoryProperty, value);
    }

    public static readonly BindableProperty ExpenseProperty =
    BindableProperty.Create(
        nameof(Expense),
        typeof(ExpenseViewModel),
        typeof(ExpenseEditorView));

    public ExpenseViewModel Expense
    {
        get => (ExpenseViewModel)GetValue(ExpenseProperty);
        set => SetValue(ExpenseProperty, value);
    }


    public ExpenseEditorView()
    {
        InitializeComponent();
    }

    public void HideKeyboard()
    {
        foreach (var entry in new[] { DescriptionEntry, CostEntry })
        {
            if (entry.Handler != null)
                _ = entry.HideKeyboardAsync(CancellationToken.None);
            entry.Unfocus();
        }
    }
}