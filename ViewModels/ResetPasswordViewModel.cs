using BillWise.Models.Services;
using BillWise.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillWise.ViewModels
{
    public partial class ResetPasswordViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        public ResetPasswordViewModel(AuthService authService)
        {
            _authService = authService;
            Title = "Reset Password";
        }

        [ObservableProperty] private string _accessToken = string.Empty;
        [ObservableProperty] private string _refreshToken = string.Empty;
        [ObservableProperty] private string _newPassword = string.Empty;
        [ObservableProperty] private string _confirmPassword = string.Empty;

        [RelayCommand]
        public async Task ResetPasswordAsync()
        {
            var L = LocalizationResourceManager.Instance;
            if (IsBusy) return;

            if (NewPassword.Length < 6)
            {
                await Shell.Current.DisplayAlertAsync(L["ErrorTitle"], L["PasswordTooShort"], "OK");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                await Shell.Current.DisplayAlertAsync(L["ErrorTitle"], L["PasswordsMustMatch"], "OK");
                return;
            }

            try
            {
                IsBusy = true;
                var (success, error) = await _authService.ResetPasswordAsync(
                    AccessToken, RefreshToken, NewPassword);

                if (success)
                {
                    await Shell.Current.DisplayAlertAsync(
                        L["ResetPassword"], L["PasswordResetSuccess"], "OK");

                    var loginPage = Application.Current?.Handler?.MauiContext?
                        .Services.GetService<Views.LoginPage>();
                    if (Application.Current?.Windows.Count > 0 && loginPage != null)
                        Application.Current.Windows[0].Page = loginPage;
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync(L["ErrorTitle"], error, "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync(L["ErrorTitle"], ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
