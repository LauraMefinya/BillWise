using BillWise.ViewModels;

namespace BillWise.Views
{
    public partial class EditProfilePage : ContentPage
    {
        public EditProfilePage(ProfileViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}