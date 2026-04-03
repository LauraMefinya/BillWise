using BillWise.Views;

namespace BillWise
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(AddInvoicePage), typeof(AddInvoicePage));
            Routing.RegisterRoute(nameof(EditProfilePage), typeof(EditProfilePage));
        }
    }
}