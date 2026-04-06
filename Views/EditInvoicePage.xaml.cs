using BillWise.ViewModels;

namespace BillWise.Views
{
    public partial class EditInvoicePage : ContentPage
    {
        public EditInvoicePage(EditInvoiceViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            if (BindingContext is EditInvoiceViewModel vm)
            {
                vm.IsDateSelected = true;
            }
        }
    }
}
