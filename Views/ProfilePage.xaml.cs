using BillWise.ViewModels;

namespace BillWise.Views
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage(ProfileViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        // Refresh stats every time page appears
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is ProfileViewModel vm)
                await vm.RefreshStatsAsync();
        }
    }
}