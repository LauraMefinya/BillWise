using BillWise.ViewModels;

namespace BillWise.Views
{
    public partial class AccountSettingsPage : ContentPage
    {
        public AccountSettingsPage(ProfileViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private async void OnBackClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("..");
    }
}
