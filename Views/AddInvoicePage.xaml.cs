using BillWise.ViewModels;

namespace BillWise.Views
{
    public partial class AddInvoicePage : ContentPage
    {
        public AddInvoicePage(AddInvoiceViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
