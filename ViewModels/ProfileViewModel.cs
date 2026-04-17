using BillWise.Models.Messages;
using BillWise.Models.Providers;
using BillWise.Models.Services;
using BillWise.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace BillWise.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly Models.Services.AuthService _authService;
        private readonly InvoiceProvider _invoiceProvider;
        private readonly Models.Services.PdfExportService _pdfExportService;
        private readonly Models.Services.InvoiceService _invoiceService;
        private readonly Models.Services.LocalNotificationScheduler _scheduler;

        public ProfileViewModel(Models.Services.AuthService authService,
                                InvoiceProvider invoiceProvider,
                                Models.Services.PdfExportService pdfExportService,
                                Models.Services.InvoiceService invoiceService,
                                Models.Services.LocalNotificationScheduler scheduler)
        {
            _authService = authService;
            _invoiceProvider = invoiceProvider;
            _pdfExportService = pdfExportService;
            _invoiceService = invoiceService;
            _scheduler = scheduler;
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
        [ObservableProperty] private string _selectedCurrency = "£";
        [ObservableProperty] private string _selectedLanguage = "en";
        [ObservableProperty] private bool _notificationsEnabled = true;
        [ObservableProperty] private int _reminderDays = 2;
        [ObservableProperty] private bool _hapticFeedbackEnabled = true;
        [ObservableProperty] private bool _shakeToAddEnabled = true;

        private void LoadSettings()
        {
            SelectedCurrency = Preferences.Default.Get("currency", "£");
            SelectedLanguage = Preferences.Default.Get("language", "en");
            NotificationsEnabled = Preferences.Default.Get("notif_enabled", true);
            ReminderDays = Preferences.Default.Get("reminder_days", 2);
            HapticFeedbackEnabled = Preferences.Default.Get("haptic_enabled", true);
            ShakeToAddEnabled = Preferences.Default.Get("shake_to_add", true);
        }

        [RelayCommand]
        public async Task SaveSettingsAsync()
        {
            var L = LocalizationResourceManager.Instance;

            Preferences.Default.Set("currency", SelectedCurrency);
            Preferences.Default.Set("language", SelectedLanguage);
            Preferences.Default.Set("notif_enabled", NotificationsEnabled);
            Preferences.Default.Set("reminder_days", ReminderDays);
            Preferences.Default.Set("haptic_enabled", HapticFeedbackEnabled);
            Preferences.Default.Set("shake_to_add", ShakeToAddEnabled);

            // Broadcast currency change so all ViewModels refresh
            WeakReferenceMessenger.Default.Send(new CurrencyChangedMessage(SelectedCurrency));

            try
            {
                var culture = new System.Globalization.CultureInfo(SelectedLanguage);
                // Broadcasts LanguageChangedMessage to all pages
                LocalizationResourceManager.Instance.SetCulture(culture);
            }
            catch { }

            // Reprogramme (ou annule) les notifications selon le nouveau réglage
            _ = _scheduler.ScheduleAsync();

            await Shell.Current.DisplayAlertAsync(L["SuccessTitle"], L["SettingsSaved"], "OK");
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task CancelChangesAsync()
        {
            LoadSettings();
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task ExportDataAsync()
        {
            var L = LocalizationResourceManager.Instance;
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                var path = await _pdfExportService.GenerateAsync(UserEmail);
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = L["ExportTitle"],
                    File  = new ShareFile(path)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF export error: {ex}");
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task ClearAllDataAsync()
        {
            var L = LocalizationResourceManager.Instance;
            bool answer = await Shell.Current.DisplayAlertAsync(
                L["WarningTitle"], L["ClearDataConfirm"], L["Yes"], L["No"]);
            if (!answer) return;

            bool deleted = await _invoiceService.DeleteAllInvoicesAsync();
            if (deleted)
            {
                Preferences.Default.Clear();
                await RefreshStatsAsync();
                await Shell.Current.DisplayAlertAsync(L["ClearedTitle"], L["DataCleared"], "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(L["WarningTitle"], "Failed to delete data. Please try again.", "OK");
            }
        }

        [RelayCommand]
        public async Task ManageCategoriesAsync()
        {
            var L = LocalizationResourceManager.Instance;
            var custom = CategoryService.GetCustomCategories();

            if (custom.Count == 0)
            {
                await Shell.Current.DisplayAlertAsync(
                    L["ManageCategories"], L["NoSavedCategories"], "OK");
                return;
            }

            string? choice = await Shell.Current.DisplayActionSheetAsync(
                L["RemoveCategoryTitle"], L["Cancel"], null, custom.ToArray());

            if (string.IsNullOrEmpty(choice) || choice == L["Cancel"]) return;

            CategoryService.RemoveCustomCategory(choice);
        }

        [RelayCommand]
        public async Task GoToEditProfileAsync() =>
            await Shell.Current.GoToAsync(nameof(Views.EditProfilePage));

        [RelayCommand]
        public async Task LogoutAsync()
        {
            var L = LocalizationResourceManager.Instance;
            var mainPage = Application.Current?.Windows[0]?.Page;
            if (mainPage == null) return;

            bool confirm = await mainPage.DisplayAlertAsync(
                L["Logout"], L["LogoutConfirm"], L["Yes"], L["No"]);
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
