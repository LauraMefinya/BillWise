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
    }
}
