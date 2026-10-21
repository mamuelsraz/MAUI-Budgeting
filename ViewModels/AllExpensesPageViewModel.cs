using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAUIBUDGET.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MAUIBUDGET.ViewModels
{
    public class AllExpensesPageViewModel : ObservableObject
    {
        public const int PageSize = 20;

        private CategoryViewModel? category;
        public CategoryViewModel? Category
        {
            get => category;
            set => SetProperty(ref category, value);
        }

        private int pageIndex;
        public int PageIndex
        {
            get => pageIndex;
            set
            {
                if (SetProperty(ref pageIndex, value))
                    OnPropertyChanged(nameof(HasPreviousPage));
            }
        }

        public bool HasPreviousPage => PageIndex > 0;

        private bool hasNextPage;
        public bool HasNextPage
        {
            get => hasNextPage;
            set => SetProperty(ref hasNextPage, value);
        }

        private bool isEmpty;
        public bool IsEmpty
        {
            get => isEmpty;
            set => SetProperty(ref isEmpty, value);
        }

        public ObservableCollection<ExpenseViewModel> Expenses { get; } = new();

        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand CloseCommand { get; }

        public event Action? PageChanged;

        private readonly ExpensesService expensesService;

        bool attached;
        bool closing;

        public AllExpensesPageViewModel(ExpensesService expensesService)
        {
            this.expensesService = expensesService;
            NextPageCommand = new RelayCommand(NextPage);
            PreviousPageCommand = new RelayCommand(PreviousPage);
            CloseCommand = new AsyncRelayCommand(Close);
        }

        public void SetCategoryByID(int id)
        {
            if (Category != null && Category.Id == id)
                return;

            var found = expensesService.Categories.FirstOrDefault(x => x.Id == id);
            if (found == null)
            {
                _ = Close();
                return;
            }

            Category = found;
            PageIndex = 0;
            Load();
        }

        public void Attach()
        {
            if (attached) return;
            attached = true;
            expensesService.Changed += OnDataChanged;

            if (Category == null) return;
            if (!expensesService.Categories.Contains(Category))
            {
                _ = Close();
                return;
            }
            Load();
        }

        public void Detach()
        {
            if (!attached) return;
            attached = false;
            expensesService.Changed -= OnDataChanged;
        }

        private void OnDataChanged(int id)
        {
            if (Category == null || id != Category.Id) return;

            if (!expensesService.Categories.Contains(Category))
            {
                _ = Close();
                return;
            }
            Load();
        }

        private void Load()
        {
            if (Category == null) return;

            var items = expensesService.GetExpensesInCategoryPage(Category.Id, PageIndex * PageSize, PageSize + 1);
            while (items.Count == 0 && PageIndex > 0)
            {
                PageIndex--;
                items = expensesService.GetExpensesInCategoryPage(Category.Id, PageIndex * PageSize, PageSize + 1);
            }

            Expenses.Clear();
            for (int i = 0; i < Math.Min(items.Count, PageSize); i++)
            {
                Expenses.Add(new ExpenseViewModel(items[i]) { IsAlternate = i % 2 == 1 });
            }

            HasNextPage = items.Count > PageSize;
            IsEmpty = items.Count == 0;
            OnPropertyChanged(nameof(HasPreviousPage));
        }

        private void NextPage()
        {
            if (!HasNextPage) return;
            PageIndex++;
            Load();
            PageChanged?.Invoke();
        }

        private void PreviousPage()
        {
            if (!HasPreviousPage) return;
            PageIndex--;
            Load();
            PageChanged?.Invoke();
        }

        private async Task Close()
        {
            if (closing) return;
            closing = true;
            await Shell.Current.GoToAsync("..");
        }
    }
}
