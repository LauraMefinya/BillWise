using BillWise.ViewModels;

namespace BillWise.Views
{
    public partial class InvoicesPage : ContentPage
    {
        private readonly InvoicesViewModel _viewModel;

        public InvoicesPage(InvoicesViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadDataCommand.ExecuteAsync("All");
        }

        private async void OnFabClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(AddInvoicePage));
        }
    }
}