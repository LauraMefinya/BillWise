using BillWise.Models.Services;
using BillWise.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillWise.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly IServiceProvider _serviceProvider;
        private readonly UserProfileService _userProfileService;

        public RegisterViewModel(AuthService authService, IServiceProvider serviceProvider, UserProfileService userProfileService)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
            _userProfileService = userProfileService;
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
                var userId = _authService.GetCurrentUserId();
                if (!string.IsNullOrEmpty(userId))
                    await _userProfileService.UpsertAsync(userId, FullName.Trim(), Email.Trim());
                if (Application.Current?.Windows.Count > 0)
                    Application.Current.Windows[0].Page = new AppShell();
            }
            else
            {
                await mainPage.DisplayAlertAsync(L["RegistrationFailed"], result.ErrorMessage, "OK");
            }
        }

        [RelayCommand]
        public Task GoBackAsync()
        {
            var loginPage = _serviceProvider.GetService<Views.LoginPage>();
            if (Application.Current?.Windows.Count > 0 && loginPage != null)
                Application.Current.Windows[0].Page = loginPage;
            return Task.CompletedTask;
        }
    }
}
