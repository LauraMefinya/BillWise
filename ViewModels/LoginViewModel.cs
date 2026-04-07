using BillWise.Models.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillWise.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        public LoginViewModel(AuthService authService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
            Title = "Login";
        }

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [RelayCommand]
        public async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                var mainPage = Application.Current?.Windows[0]?.Page;
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Error", "Please enter your email and password.", "OK");
                return;
            }

            IsBusy = true;
            
            var result = await _authService.LoginAsync(Email.Trim(), Password);
            
            IsBusy = false;

            if (result.Success)
            {
                // Navigate to main application shell
                if (Application.Current != null && Application.Current.Windows.Count > 0)
                {
                    Application.Current.Windows[0].Page = new AppShell();
                }
            }
            else
            {
                var mainPage = Application.Current?.Windows[0]?.Page;
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Login Failed", result.ErrorMessage, "OK");
            }
        }

        [RelayCommand]
        public async Task GoToRegisterAsync()
        {
            var registerPage = _serviceProvider.GetService<Views.RegisterPage>();
            var mainPage = Application.Current?.Windows[0]?.Page;
            if (mainPage != null && registerPage != null)
                await mainPage.Navigation.PushAsync(registerPage);
        }
    }
}
