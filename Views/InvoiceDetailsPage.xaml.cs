using BillWise.ViewModels;

namespace BillWise.Views
{
    public partial class InvoiceDetailsPage : ContentPage
    {
        public InvoiceDetailsPage(InvoiceDetailsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
