using BillWise.Models.Services;
using BillWise.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillWise.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly IServiceProvider _serviceProvider;
        private readonly UserProfileService _userProfileService;

        public LoginViewModel(AuthService authService, IServiceProvider serviceProvider, UserProfileService userProfileService)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
            _userProfileService = userProfileService;
            Title = "Login";
        }

        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private string _password = string.Empty;

        [RelayCommand]
        public async Task LoginAsync()
        {
            var L = LocalizationResourceManager.Instance;
            var mainPage = Application.Current?.Windows[0]?.Page;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync(L["ErrorTitle"], L["EnterEmailAndPassword"], "OK");
                return;
            }

            IsBusy = true;
            var result = await _authService.LoginAsync(Email.Trim(), Password);
            IsBusy = false;

            if (result.Success)
            {
                var userId = _authService.GetCurrentUserId();
                if (!string.IsNullOrEmpty(userId))
                {
                    var name = await _userProfileService.FetchNameAsync(userId);
                    if (!string.IsNullOrEmpty(name))
                        Preferences.Default.Set("user_name", name);
                }
                if (Application.Current != null && Application.Current.Windows.Count > 0)
                    Application.Current.Windows[0].Page = new AppShell();
            }
            else
            {
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync(L["LoginFailed"], result.ErrorMessage, "OK");
            }
        }

        [RelayCommand]
        public async Task ForgotPasswordAsync()
        {
            var L = LocalizationResourceManager.Instance;
            var mainPage = Application.Current?.Windows[0]?.Page;
            if (mainPage == null) return;

            var email = await mainPage.DisplayPromptAsync(
                L["ForgotPassword"],
                L["EnterEmailForReset"],
                L["Send"],
                L["Cancel"],
                keyboard: Keyboard.Email);

            if (string.IsNullOrWhiteSpace(email)) return;

            IsBusy = true;
            var result = await _authService.ForgotPasswordAsync(email.Trim());
            IsBusy = false;

            if (result.Success)
                await mainPage.DisplayAlertAsync(L["SuccessTitle"], L["ResetEmailSent"], "OK");
            else
                await mainPage.DisplayAlertAsync(L["ErrorTitle"], result.ErrorMessage, "OK");
        }

        [RelayCommand]
        public Task GoToRegisterAsync()
        {
            var registerPage = _serviceProvider.GetService<Views.RegisterPage>();
            if (Application.Current?.Windows.Count > 0 && registerPage != null)
                Application.Current.Windows[0].Page = registerPage;
            return Task.CompletedTask;
        }
    }
}
