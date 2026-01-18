using LiteDB;
using MAUIBUDGET.Models;
using MAUIBUDGET.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MAUIBUDGET.Services
{
    public class ExpensesService
    {
        public ObservableCollection<CategoryViewModel> Categories { get; private set; } = new();

        string dbPath => Path.Combine(FileSystem.AppDataDirectory, "app.db");

        public ExpensesService()
        {
#if DEBUG
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
#endif
            CheckOrCreateDB();
            LoadCategories();
#if DEBUG
            CreateDummyData();
#endif
        }

        void CheckOrCreateDB()
        {
            Debug.WriteLine("Creating DB at " + dbPath);

            using (var db = new LiteDatabase(dbPath))
            {
                ILiteCollection<Category> categories = db.GetCollection<Category>("categories");
                ILiteCollection<Expense> expenses = db.GetCollection<Expense>("expenses");
                ILiteCollection<Expense> notifExpenses = db.GetCollection<Expense>("notif_expenses");

                categories.EnsureIndex(c => c.Name);
                categories.EnsureIndex(c => c.HasBudget);
                categories.EnsureIndex(c => c.Budget);
                categories.EnsureIndex(c => c.BudgetPeriod);
                categories.EnsureIndex(c => c.Name);

                expenses.EnsureIndex(e => e.Description);
                expenses.EnsureIndex(e => e.Date);
                expenses.EnsureIndex(e => e.Cost);
                expenses.EnsureIndex(e => e.CategoryId);

                notifExpenses.EnsureIndex(e => e.Description);
                notifExpenses.EnsureIndex(e => e.Date);
                notifExpenses.EnsureIndex(e => e.Cost);

                if (!categories.Exists(Query.All())) // No categories present, create sample categories
                {
                    categories.Insert(new Category { Name = "Lunch", HasBudget = true, Budget = 150, BudgetPeriod = Period.WorkDay });
                    categories.Insert(new Category { Name = "Transport", HasBudget = true, Budget = 2000, BudgetPeriod = Period.Month });
                    categories.Insert(new Category { Name = "Other", HasBudget = false });
                }
            }
        }

        void CreateDummyData()
        {
            Random rnd = new Random();

            foreach (var category in Categories)
            {

                double budget = 100 + rnd.NextDouble() * 200;

                if (category.HasBudget)
                    budget = category.BudgetPerDay;

                DateTime date = DateTime.Now;
                for (int i = 0; i < 50; i++)
                {
                    TimeSpan t = TimeSpan.FromDays(0.5 + rnd.NextDouble() * 1.5);
                    double spent = ((budget / 3) + rnd.NextDouble() * budget*2) / t.TotalDays;
                    date -= t;

                    using (var db = new LiteDatabase(dbPath))
                    {
                        ILiteCollection<Expense> expenses = db.GetCollection<Expense>("expenses");
                        expenses.Insert(new Expense
                        {
                            Description = "Dummy Expense",
                            Cost = spent,
                            Date = date.ToUniversalTime(),
                            CategoryId = category.Id,
                        });
                    }
                }
            }

            LoadCategories();
        }

        public void LoadCategories()
        {
            Categories.Clear();

            List<CategoryViewModel> tempCategories = new List<CategoryViewModel>();

            using (var db = new LiteDatabase(dbPath))
            {
                ILiteCollection<Category> categoryCollection = db.GetCollection<Category>("categories");
                ILiteCollection<Expense> expensesCollection = db.GetCollection<Expense>("expenses");

                var dbCategories = categoryCollection.FindAll();
                foreach (var category in dbCategories)
                {
                    Debug.WriteLine($"Loading category {category.Name} from DB.");

                    CategoryViewModel categoryViewModel = new CategoryViewModel(category);

                    var dbLastExpenses = expensesCollection
                        .Find(Query.EQ("CategoryId", category.Id))
                        .OrderByDescending(x => x.Date)
                        .Take(3);

                    foreach (var expense in dbLastExpenses)
                    {
                        categoryViewModel.LastExpenses.Add(new ExpenseViewModel(expense));
                    }

                    tempCategories.Add(categoryViewModel);
                }
            }

            foreach (var item in tempCategories)
            {
                Categories.Add(item);
            }
        }

        public void AddCategory(CategoryViewModel category)
        {
            using (var db = new LiteDatabase(dbPath))
            {
                ILiteCollection<Category> categories = db.GetCollection<Category>("categories");

                int id = categories.Insert(new Category
                {
                    Name = category.Name,
                    HasBudget = category.HasBudget,
                    Budget = category.Budget,
                    BudgetPeriod = category.BudgetPeriod
                }).AsInt32;
                category.Id = id;
            }

            Categories.Add(category);
        }

        public void DeleteCategory(int categoryID)
        {
            var category = Categories.FirstOrDefault(x => x.Id == categoryID);
            if (category == null)
                throw new InvalidDataException("Category is not valid");

            using (var db = new LiteDatabase(dbPath))
            {
                ILiteCollection<Category> categories = db.GetCollection<Category>("categories");
                categories.Delete(categoryID);

                ILiteCollection<Expense> expenses = db.GetCollection<Expense>("expenses");
                expenses.DeleteMany(Query.EQ("CategoryId", categoryID));
            }

            Categories.Remove(category);
        }

        public void EditCategory(int categoryID, CategoryViewModel newCategory)
        {
            var editedCategory = Categories.FirstOrDefault(x => x.Id == categoryID);
            if (editedCategory == null)
                throw new InvalidDataException("Category is not valid");
            if (editedCategory.Id != categoryID)
                throw new InvalidDataException("Category has invalid ID");

            using (var db = new LiteDatabase(dbPath))
            {
                ILiteCollection<Category> categories = db.GetCollection<Category>("categories");
                Category dbCategory = new Category()
                {
                    Id = categoryID,
                    Name = newCategory.Name,
                    HasBudget = newCategory.HasBudget,
                    Budget = newCategory.Budget,
                    BudgetPeriod = newCategory.BudgetPeriod
                };
                categories.Update(categoryID, dbCategory);
            }

            editedCategory.Name = newCategory.Name;
            editedCategory.HasBudget = newCategory.HasBudget;
            editedCategory.Budget = newCategory.Budget;
            editedCategory.BudgetPeriod = newCategory.BudgetPeriod;
        }

        public void AddExpense(ExpenseViewModel expense, CategoryViewModel category)
        {
            if (!Categories.Contains(category))
                throw new InvalidDataException("Category is not valid");

            using (var db = new LiteDatabase(dbPath))
            {
                ILiteCollection<Expense> expenses = db.GetCollection<Expense>("expenses");
                expenses.Insert(new Expense
                {
                    Description = expense.Description,
                    Cost = expense.Cost,
                    Date = expense.Date.ToUniversalTime(),
                    CategoryId = category.Id,
                });
            }

            LoadCategories();
        }

        public int PushNotificationExpense(ExpenseViewModel expense)
        {
            int id = -1;
            using (var db = new LiteDatabase(dbPath))
            {
                ILiteCollection<Expense> notifExpenses = db.GetCollection<Expense>("notif_expenses");
                id = notifExpenses.Insert(new Expense
                {
                    Description = expense.Description,
                    Cost = expense.Cost,
                    Date = expense.Date.ToUniversalTime(),
                });
            }

            return id;
        }

        public Expense? PopNotificationExpense(int id)
        {
            Expense? expense = null;
            using (var db = new LiteDatabase(dbPath))
            {
                ILiteCollection<Expense> expenses = db.GetCollection<Expense>("notif_expenses");
                expense = expenses.FindById(id);
                expenses.Delete(id);
            }

            return expense;
        }

        public IEnumerable<Expense> GetExpensesInCategory(int categoryId, DateTime fromDate, DateTime toDate)
        {
            fromDate = fromDate.ToUniversalTime();
            toDate = toDate.ToUniversalTime();
            using (var db = new LiteDatabase(dbPath))
            {
                ILiteCollection<Expense> expensesCollection = db.GetCollection<Expense>("expenses");

                return expensesCollection
                    .Find(Query.And(Query.EQ("CategoryId", categoryId), Query.Between("Date", fromDate, toDate)))
                    .OrderBy(x => x.Date)
                    .ToList();
            }
        }

        public double GetOverallSpendingInPeriod(DateTime fromDate, DateTime toDate)
        {
            fromDate = fromDate.ToUniversalTime();
            toDate = toDate.ToUniversalTime();

            using (var db = new LiteDatabase(dbPath))
            {
                ILiteCollection<Expense> expensesCollection = db.GetCollection<Expense>("expenses");

                return expensesCollection
                    .Find(Query.Between("Date", fromDate, toDate))
                    .Sum(x => x.Cost);
            }
        }
    }
}