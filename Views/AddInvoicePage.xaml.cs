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

        private void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            if (BindingContext is AddInvoiceViewModel vm)
            {
                vm.IsDateSelected = true;
            }
        }
    }
}
