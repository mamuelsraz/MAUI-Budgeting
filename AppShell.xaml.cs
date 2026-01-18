namespace MAUIBUDGET
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("newcategory", typeof(AddCategoryPage));
            Routing.RegisterRoute("editcategory", typeof(EditCategoryPage));
        }
    }
}
