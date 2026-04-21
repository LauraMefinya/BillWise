using BillWise.Models.Services;
using BillWise.Resources.Strings;
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

        [ObservableProperty] private string _fullName = string.Empty;
        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private string _password = string.Empty;
        [ObservableProperty] private string _confirmPassword = string.Empty;

        [RelayCommand]
        public async Task RegisterAsync()
        {
            var L = LocalizationResourceManager.Instance;
            var mainPage = Application.Current?.Windows[0]?.Page;

            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync(L["ErrorTitle"], L["EnterEmailAndPassword"], "OK");
                return;
            }

            if (Password != ConfirmPassword)
            {
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync(L["ErrorTitle"], L["PasswordsDoNotMatch"], "OK");
                return;
            }

            IsBusy = true;
            var result = await _authService.RegisterAsync(Email.Trim(), Password);
            IsBusy = false;

            if (mainPage == null) return;

            if (result.Success)
            {
                Preferences.Default.Set("user_name", FullName.Trim());
                if (Application.Current?.Windows.Count > 0)
                    Application.Current.Windows[0].Page = new AppShell();
            }
            else
            {
                await mainPage.DisplayAlertAsync(L["RegistrationFailed"], result.ErrorMessage, "OK");
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
