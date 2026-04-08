using BillWise.Models.Messages;using BillWise.Models.Providers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillWise.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly Models.Services.AuthService _authService;
        private readonly InvoiceProvider _invoiceProvider;

        public ProfileViewModel(Models.Services.AuthService authService,
                                InvoiceProvider invoiceProvider)
        {
            _authService = authService;
            _invoiceProvider = invoiceProvider;
            Title = "Profile";
            LoadSettings();
            LoadUserData();

            // Listen for invoice changes from any page
            _invoiceProvider.InvoicesChanged += async () =>
                await RefreshStatsAsync();
        }

        // Override to reload settings when language changes
        public override void Receive(LanguageChangedMessage message)
        {
            base.Receive(message);
            LoadSettings();
        }

        private async void LoadUserData()
        {
            var email = _authService.GetCurrentUserEmail();
            UserEmail = !string.IsNullOrEmpty(email) ? email : "Connected User";
            if (!string.IsNullOrEmpty(email))
                UserName = email.Split('@')[0];
            await RefreshStatsAsync();
        }

        // Public so ProfilePage.OnAppearing can call it
        public async Task RefreshStatsAsync()
        {
            await _invoiceProvider.GetInvoicesAsync(forceRefresh: true);
            TotalInvoices = _invoiceProvider.TotalCount;
            PaidInvoices = _invoiceProvider.PaidCount;
            PendingInvoices = _invoiceProvider.PendingCount;
            OverdueInvoices = _invoiceProvider.OverdueCount;
        }

        [ObservableProperty] private string _userName = "User";
        [ObservableProperty] private string _userEmail = "user@example.com";
        [ObservableProperty] private int _totalInvoices = 0;
        [ObservableProperty] private int _paidInvoices = 0;
        [ObservableProperty] private int _pendingInvoices = 0;
        [ObservableProperty] private int _overdueInvoices = 0;
        [ObservableProperty] private string _selectedCurrency = "FCFA";
        [ObservableProperty] private string _selectedLanguage = "en";
        [ObservableProperty] private bool _notificationsEnabled = true;
        [ObservableProperty] private int _reminderDays = 2;
        [ObservableProperty] private bool _hapticFeedbackEnabled = true;
        [ObservableProperty] private bool _shakeToAddEnabled = true;

        private void LoadSettings()
        {
            SelectedCurrency = Preferences.Default.Get("currency", "FCFA");
            SelectedLanguage = Preferences.Default.Get("language", "en");
            NotificationsEnabled = Preferences.Default.Get("notif_enabled", true);
            ReminderDays = Preferences.Default.Get("reminder_days", 2);
            HapticFeedbackEnabled = Preferences.Default.Get("haptic_enabled", true);
            ShakeToAddEnabled = Preferences.Default.Get("shake_to_add", true);
        }

        [RelayCommand]
        public async Task SaveSettingsAsync()
        {
            Preferences.Default.Set("currency", SelectedCurrency);
            Preferences.Default.Set("language", SelectedLanguage);
            Preferences.Default.Set("notif_enabled", NotificationsEnabled);
            Preferences.Default.Set("reminder_days", ReminderDays);
            Preferences.Default.Set("haptic_enabled", HapticFeedbackEnabled);
            Preferences.Default.Set("shake_to_add", ShakeToAddEnabled);

            try
            {
                var culture = new System.Globalization.CultureInfo(SelectedLanguage);
                // This now broadcasts LanguageChangedMessage to all pages
                BillWise.Resources.Strings.LocalizationResourceManager
                    .Instance.SetCulture(culture);
            }
            catch { }

            await Shell.Current.DisplayAlertAsync("Success", "Settings saved.", "OK");
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task CancelChangesAsync()
        {
            // Revert changes and go back
            LoadSettings();
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task ExportDataAsync() =>
            await Shell.Current.DisplayAlertAsync("Export",
                "Export feature coming soon.", "OK");

        [RelayCommand]
        public async Task ClearAllDataAsync()
        {
            bool answer = await Shell.Current.DisplayAlertAsync("Warning",
                "Are you sure you want to clear all data?", "Yes", "No");
            if (answer)
            {
                Preferences.Default.Clear();
                await Shell.Current.DisplayAlertAsync("Cleared",
                    "All data cleared.", "OK");
            }
        }

        [RelayCommand]
        public async Task GoToEditProfileAsync() =>
            await Shell.Current.GoToAsync(nameof(Views.EditProfilePage));

        [RelayCommand]
        public async Task LogoutAsync()
        {
            var mainPage = Application.Current?.Windows[0]?.Page;
            if (mainPage == null) return;

            bool confirm = await mainPage.DisplayAlertAsync("Logout",
                "Are you sure you want to log out?", "Yes", "No");
            if (confirm)
            {
                await _authService.LogoutAsync();
                var loginPage = Application.Current.Handler.MauiContext
                    .Services.GetService<Views.LoginPage>();
                if (Application.Current?.Windows.Count > 0)
                    Application.Current.Windows[0].Page =
                        new NavigationPage(loginPage);
            }
        }
    }
}