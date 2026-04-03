using BillWise.ViewModels;

namespace BillWise.Views
{
    public partial class HomePage : ContentPage
    {
        private readonly HomeViewModel _viewModel;

        public HomePage(HomeViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadDataAsync();
        }

        private async void OnFabClicked(object sender, EventArgs e)
        {
            // Navigate to AddInvoicePage
            await Shell.Current.GoToAsync(nameof(AddInvoicePage));
        }
    }
}