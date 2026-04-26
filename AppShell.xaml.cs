using BillWise.Views;

namespace BillWise
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(AddInvoicePage), typeof(AddInvoicePage));
            Routing.RegisterRoute(nameof(AccountSettingsPage), typeof(AccountSettingsPage));
            Routing.RegisterRoute(nameof(EditProfilePage), typeof(EditProfilePage));
            Routing.RegisterRoute(nameof(InvoiceDetailsPage), typeof(InvoiceDetailsPage));
            Routing.RegisterRoute(nameof(EditInvoicePage), typeof(EditInvoicePage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(AlertsPage), typeof(AlertsPage));
        }

    }
}