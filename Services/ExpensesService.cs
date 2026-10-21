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

        public event Action<int>? Changed;

        string dbPath => Path.Combine(FileSystem.AppDataDirectory, "app.db");

        const string NextNotificationIdKey = "NextNotificationId";

        readonly LiteDatabase db;

        public ExpensesService()
        {
#if DEBUG
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
#endif
            db = new LiteDatabase(dbPath);

            CheckOrCreateDB();
            LoadCategories();
#if DEBUG
            CreateDummyData();
#endif
        }

        void CheckOrCreateDB()
        {
            Debug.WriteLine("Creating DB at " + dbPath);

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

            if (!categories.Exists(Query.All()))
            {
                categories.Insert(new Category { Name = "Lunch", HasBudget = true, Budget = 150, BudgetPeriod = Period.WorkDay });
                categories.Insert(new Category { Name = "Transport", HasBudget = true, Budget = 2000, BudgetPeriod = Period.Month });
                categories.Insert(new Category { Name = "Other", HasBudget = false });
            }
        }

        void CreateDummyData()
        {
            Random rnd = new Random();
            List<Expense> dummyExpenses = new();

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

                    dummyExpenses.Add(new Expense
                    {
                        Description = "Dummy Expense",
                        Cost = spent,
                        Date = date.ToUniversalTime(),
                        CategoryId = category.Id,
                    });
                }
            }

            db.GetCollection<Expense>("expenses").InsertBulk(dummyExpenses);

            LoadCategories();
        }

        void LoadCategories()
        {
            Categories.Clear();

            List<CategoryViewModel> tempCategories = new List<CategoryViewModel>();

            ILiteCollection<Category> categoryCollection = db.GetCollection<Category>("categories");

            var dbCategories = categoryCollection.FindAll();
            foreach (var category in dbCategories)
            {
                Debug.WriteLine($"Loading category {category.Name} from DB.");

                CategoryViewModel categoryViewModel = new CategoryViewModel(category);
                RefreshLastExpenses(categoryViewModel);

                tempCategories.Add(categoryViewModel);
            }

            foreach (var item in tempCategories)
            {
                Categories.Add(item);
            }
        }

        void RefreshLastExpenses(CategoryViewModel category)
        {
            var dbLastExpenses = db.GetCollection<Expense>("expenses")
                .Find(Query.EQ("CategoryId", category.Id))
                .OrderByDescending(x => x.Date)
                .Take(3)
                .ToList();

            category.LastExpenses.Clear();
            for (int i = 0; i < dbLastExpenses.Count; i++)
            {
                category.LastExpenses.Add(new ExpenseViewModel(dbLastExpenses[i]) { IsAlternate = i % 2 == 1 });
            }
        }

        public void AddCategory(CategoryViewModel category)
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

            Categories.Add(category);
            Changed?.Invoke(category.Id);
        }

        public void DeleteCategory(int categoryID)
        {
            var category = Categories.FirstOrDefault(x => x.Id == categoryID);
            if (category == null)
                throw new InvalidDataException("Category is not valid");

            ILiteCollection<Category> categories = db.GetCollection<Category>("categories");
            categories.Delete(categoryID);

            ILiteCollection<Expense> expenses = db.GetCollection<Expense>("expenses");
            expenses.DeleteMany(Query.EQ("CategoryId", categoryID));

            Categories.Remove(category);
            Changed?.Invoke(categoryID);
        }

        public void EditCategory(int categoryID, CategoryViewModel newCategory)
        {
            var editedCategory = Categories.FirstOrDefault(x => x.Id == categoryID);
            if (editedCategory == null)
                throw new InvalidDataException("Category is not valid");
            if (editedCategory.Id != categoryID)
                throw new InvalidDataException("Category has invalid ID");

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

            editedCategory.Name = newCategory.Name;
            editedCategory.HasBudget = newCategory.HasBudget;
            editedCategory.Budget = newCategory.Budget;
            editedCategory.BudgetPeriod = newCategory.BudgetPeriod;

            Changed?.Invoke(categoryID);
        }

        public void AddExpense(ExpenseViewModel expense, CategoryViewModel category)
        {
            var target = Categories.FirstOrDefault(c => c.Id == category.Id)
                ?? throw new InvalidDataException("Category is not valid");

            ILiteCollection<Expense> expenses = db.GetCollection<Expense>("expenses");
            expenses.Insert(new Expense
            {
                Description = expense.Description,
                Cost = expense.Cost,
                Date = expense.Date.ToUniversalTime(),
                CategoryId = target.Id,
            });

            RefreshLastExpenses(target);
            Changed?.Invoke(target.Id);
        }

        public Expense? GetExpense(int id)
        {
            return db.GetCollection<Expense>("expenses").FindById(id);
        }

        public void UpdateExpense(ExpenseViewModel expense, CategoryViewModel category)
        {
            var target = Categories.FirstOrDefault(c => c.Id == category.Id)
                ?? throw new InvalidDataException("Category is not valid");

            ILiteCollection<Expense> expenses = db.GetCollection<Expense>("expenses");
            Expense existing = expenses.FindById(expense.Id)
                ?? throw new InvalidDataException("Expense is not valid");
            int oldCategoryId = existing.CategoryId;

            expenses.Update(new Expense
            {
                Id = expense.Id,
                Description = expense.Description,
                Cost = expense.Cost,
                Date = expense.Date.ToUniversalTime(),
                CategoryId = target.Id,
            });

            RefreshLastExpenses(target);
            var oldCategory = oldCategoryId != target.Id
                ? Categories.FirstOrDefault(c => c.Id == oldCategoryId)
                : null;
            if (oldCategory != null)
                RefreshLastExpenses(oldCategory);

            if (oldCategoryId != target.Id)
                Changed?.Invoke(oldCategoryId);
            Changed?.Invoke(target.Id);
        }

        public void DeleteExpense(int id)
        {
            ILiteCollection<Expense> expenses = db.GetCollection<Expense>("expenses");
            Expense? existing = expenses.FindById(id);
            if (existing == null)
                return;

            expenses.Delete(id);

            var category = Categories.FirstOrDefault(c => c.Id == existing.CategoryId);
            if (category != null)
                RefreshLastExpenses(category);
            Changed?.Invoke(existing.CategoryId);
        }

        public int PushNotificationExpense(ExpenseViewModel expense)
        {
            db.BeginTrans();
            try
            {
                ILiteCollection<BsonDocument> meta = db.GetCollection("meta");
                ILiteCollection<Expense> notifExpenses = db.GetCollection<Expense>("notif_expenses");

                BsonDocument? counter = meta.FindById(NextNotificationIdKey);
                int id;
                if (counter == null)
                {
                    id = notifExpenses.Count() == 0 ? 1 : notifExpenses.Max(x => x.Id) + 1;
                    if (id <= 0)
                        id = 1;
                }
                else id = counter["value"].AsInt32;

                int next = id == int.MaxValue ? 1 : id + 1;
                meta.Upsert(new BsonDocument { ["_id"] = NextNotificationIdKey, ["value"] = next });

                notifExpenses.Insert(new Expense
                {
                    Id = id,
                    Description = expense.Description,
                    Cost = expense.Cost,
                    Date = expense.Date.ToUniversalTime(),
                });

                db.Commit();
                return id;
            }
            catch
            {
                db.Rollback();
                throw;
            }
        }

        public Expense? PopNotificationExpense(int id)
        {
            ILiteCollection<Expense> expenses = db.GetCollection<Expense>("notif_expenses");
            Expense? expense = expenses.FindById(id);
            expenses.Delete(id);

            return expense;
        }

        public IEnumerable<Expense> GetExpensesInCategory(int categoryId, DateTime fromDate, DateTime toDate)
        {
            fromDate = fromDate.ToUniversalTime();
            toDate = toDate.ToUniversalTime();

            ILiteCollection<Expense> expensesCollection = db.GetCollection<Expense>("expenses");

            return expensesCollection
                .Find(Query.And(Query.EQ("CategoryId", categoryId), Query.Between("Date", fromDate, toDate)))
                .OrderBy(x => x.Date)
                .ToList();
        }

        public List<Expense> GetExpensesInCategoryPage(int categoryId, int skip, int take)
        {
            return db.GetCollection<Expense>("expenses")
                .Find(Query.EQ("CategoryId", categoryId))
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.Id)
                .Skip(skip)
                .Take(take)
                .ToList();
        }

        public double GetOverallSpendingInPeriod(DateTime fromDate, DateTime toDate)
        {
            fromDate = fromDate.ToUniversalTime();
            toDate = toDate.ToUniversalTime();

            ILiteCollection<Expense> expensesCollection = db.GetCollection<Expense>("expenses");

            return expensesCollection
                .Find(Query.Between("Date", fromDate, toDate))
                .Sum(x => x.Cost);
        }
    }
}
