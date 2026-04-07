using BillWise.Models.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillWise.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        public RegisterViewModel(AuthService authService)
        {
            _authService = authService;
            Title = "Register";
        }

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        [RelayCommand]
        public async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                var mainPage = Application.Current?.Windows[0]?.Page;
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Error", "Please enter your email and password.", "OK");
                return;
            }

            if (Password != ConfirmPassword)
            {
                var mainPage = Application.Current?.Windows[0]?.Page;
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Error", "Passwords do not match.", "OK");
                return;
            }

            IsBusy = true;
            
            var result = await _authService.RegisterAsync(Email.Trim(), Password);
            
            IsBusy = false;

            if (result.Success)
            {
                var mainPage = Application.Current?.Windows[0]?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Success", "Account created successfully! You can now log in.", "OK");
                    await mainPage.Navigation.PopAsync(); // Go back to login
                }
            }
            else
            {
                var mainPage = Application.Current?.Windows[0]?.Page;
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Registration Failed", result.ErrorMessage, "OK");
            }
        }

        [RelayCommand]
        public async Task GoBackAsync()
        {
            var mainPage = Application.Current?.Windows[0]?.Page;
            if (mainPage != null)
                await mainPage.Navigation.PopAsync();
        }
    }
}
