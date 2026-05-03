using BillWise.Models.Messages;
using BillWise.Models.Providers;
using BillWise.Models.Services;
using BillWise.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui;

namespace BillWise.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly InvoiceProvider _invoiceProvider;
        private readonly PdfExportService _pdfExportService;
        private readonly InvoiceService _invoiceService;
        private readonly LocalNotificationScheduler _scheduler;
        private readonly UserProfileService _userProfileService;

        private bool _isLoading;

        public List<string> CurrencyOptions { get; } = new() { "$", "€", "£" };
        public List<string> LanguageOptions { get; } = new() { "en", "fr" };

        public ProfileViewModel(AuthService authService,
                                InvoiceProvider invoiceProvider,
                                PdfExportService pdfExportService,
                                InvoiceService invoiceService,
                                LocalNotificationScheduler scheduler,
                                UserProfileService userProfileService)
        {
            _authService = authService;
            _invoiceProvider = invoiceProvider;
            _pdfExportService = pdfExportService;
            _invoiceService = invoiceService;
            _scheduler = scheduler;
            _userProfileService = userProfileService;
            Title = "Profile";
            LoadSettings();
            LoadUserData();

            _invoiceProvider.InvoicesChanged += async () =>
                await RefreshStatsAsync();
        }

        public override void Receive(LanguageChangedMessage message)
        {
            base.Receive(message);
            LoadSettings();
        }

        private async void LoadUserData()
        {
            var email = _authService.GetCurrentUserEmail();
            UserEmail = !string.IsNullOrEmpty(email) ? email : "Connected User";
            var savedName = Preferences.Default.Get("user_name", string.Empty);
            UserName = !string.IsNullOrEmpty(savedName)
                ? savedName
                : (!string.IsNullOrEmpty(email) ? email.Split('@')[0] : "User");
            var savedPhoto = Preferences.Default.Get("profile_photo_path", string.Empty);
            ProfilePhotoPath = string.IsNullOrEmpty(savedPhoto) ? null : savedPhoto;
            await RefreshStatsAsync();
        }

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
        [ObservableProperty] private string? _profilePhotoPath;

        public bool HasProfilePhoto => !string.IsNullOrEmpty(ProfilePhotoPath);
        public bool HasNoProfilePhoto => string.IsNullOrEmpty(ProfilePhotoPath);
        public ImageSource? ProfilePhoto =>
            string.IsNullOrEmpty(ProfilePhotoPath) ? null : ImageSource.FromFile(ProfilePhotoPath);

        partial void OnProfilePhotoPathChanged(string? value)
        {
            OnPropertyChanged(nameof(HasProfilePhoto));
            OnPropertyChanged(nameof(HasNoProfilePhoto));
            OnPropertyChanged(nameof(ProfilePhoto));
        }
        [ObservableProperty] private int _totalInvoices;
        [ObservableProperty] private int _paidInvoices;
        [ObservableProperty] private int _pendingInvoices;
        [ObservableProperty] private int _overdueInvoices;
        [ObservableProperty] private string _selectedCurrency = "£";
        [ObservableProperty] private string _selectedLanguage = "en";
        [ObservableProperty] private bool _notificationsEnabled = true;
        [ObservableProperty] private int _reminderDays = 2;
        [ObservableProperty] private bool _hapticFeedbackEnabled = true;
        [ObservableProperty] private bool _shakeToAddEnabled = true;
        [ObservableProperty] private bool _darkModeEnabled;

        public string NotificationsStatusText => LocalizationResourceManager.Instance[NotificationsEnabled ? "Enabled" : "Disabled"];
        public string HapticStatusText => LocalizationResourceManager.Instance[HapticFeedbackEnabled ? "Enabled" : "Disabled"];

        partial void OnSelectedCurrencyChanged(string value)
        {
            if (_isLoading) return;
            Preferences.Default.Set("currency", value);
            WeakReferenceMessenger.Default.Send(new CurrencyChangedMessage(value));
        }

        partial void OnSelectedLanguageChanged(string value)
        {
            if (_isLoading) return;
            Preferences.Default.Set("language", value);
            try
            {
                LocalizationResourceManager.Instance.SetCulture(
                    new System.Globalization.CultureInfo(value));
            }
            catch { }
        }

        partial void OnNotificationsEnabledChanged(bool value)
        {
            OnPropertyChanged(nameof(NotificationsStatusText));
            if (_isLoading) return;
            Preferences.Default.Set("notif_enabled", value);
            _ = _scheduler.ScheduleAsync();
        }

        partial void OnHapticFeedbackEnabledChanged(bool value)
        {
            OnPropertyChanged(nameof(HapticStatusText));
            if (_isLoading) return;
            Preferences.Default.Set("haptic_enabled", value);
        }

        partial void OnDarkModeEnabledChanged(bool value)
        {
            if (Application.Current != null)
                Application.Current.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
            if (_isLoading) return;
            Preferences.Default.Set("dark_mode", value);
        }

        private void LoadSettings()
        {
            _isLoading = true;
            SelectedCurrency = Preferences.Default.Get("currency", "£");
            SelectedLanguage = Preferences.Default.Get("language", "en");
            NotificationsEnabled = Preferences.Default.Get("notif_enabled", true);
            ReminderDays = Preferences.Default.Get("reminder_days", 2);
            HapticFeedbackEnabled = Preferences.Default.Get("haptic_enabled", true);
            ShakeToAddEnabled = Preferences.Default.Get("shake_to_add", true);
            DarkModeEnabled = Preferences.Default.Get("dark_mode", false);
            _isLoading = false;
        }

        [RelayCommand]
        public async Task PickProfilePhotoAsync()
        {
            var L = LocalizationResourceManager.Instance;
            string? choice = await Shell.Current.DisplayActionSheetAsync(
                null, L["Cancel"], null, "📷 Take Photo", "🖼 Choose from Library");

            if (string.IsNullOrEmpty(choice) || choice == L["Cancel"]) return;

            try
            {
                FileResult? photo = choice.Contains("Take Photo")
                    ? await MediaPicker.Default.CapturePhotoAsync()
                    : await MediaPicker.Default.PickPhotoAsync();

                if (photo == null) return;

                // Delete old file to avoid cache issues
                var oldPath = Preferences.Default.Get("profile_photo_path", string.Empty);
                if (!string.IsNullOrEmpty(oldPath) && File.Exists(oldPath))
                    File.Delete(oldPath);

                // Unique filename so the binding always sees a new value
                var localPath = Path.Combine(
                    FileSystem.AppDataDirectory,
                    $"profile_{DateTime.Now.Ticks}.jpg");

                using var src = await photo.OpenReadAsync();
                using var dst = File.Create(localPath);
                await src.CopyToAsync(dst);

                ProfilePhotoPath = null; // force binding reset
                ProfilePhotoPath = localPath;
                Preferences.Default.Set("profile_photo_path", localPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProfileVM] Photo error: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task ShowNotificationSettingsAsync()
        {
            var popup = new Views.Popups.NotificationSettingsPopup(this);
            await Shell.Current.ShowPopupAsync(popup);
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

            var labels = custom.Select(c => $"{c.Icon} {c.Name}").ToArray();
            string? choice = await Shell.Current.DisplayActionSheetAsync(
                L["RemoveCategoryTitle"], L["Cancel"], null, labels);

            if (string.IsNullOrEmpty(choice) || choice == L["Cancel"]) return;

            // Extract the name part after the icon prefix
            var name = choice.Contains(' ') ? choice[(choice.IndexOf(' ') + 1)..] : choice;
            CategoryService.RemoveCustomCategory(name);
        }

        [RelayCommand]
        public async Task GoToEditProfileAsync() =>
            await Shell.Current.GoToAsync(nameof(Views.EditProfilePage));

        [RelayCommand]
        public async Task GoToAccountSettingsAsync() =>
            await Shell.Current.GoToAsync(nameof(Views.AccountSettingsPage));

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
                    Application.Current.Windows[0].Page = loginPage;
            }
        }

        [RelayCommand]
        public async Task DeleteAccountAsync()
        {
            var L = LocalizationResourceManager.Instance;
            var mainPage = Application.Current?.Windows[0]?.Page;
            if (mainPage == null) return;

            bool confirm = await mainPage.DisplayAlertAsync(
                L["DeleteAccount"],
                L["DeleteAccountConfirm"],
                L["Yes"], L["No"]);
            if (!confirm) return;

            try
            {
                IsBusy = true;

                // The RPC deletes invoices then the auth user in one transaction
                var (deleted, error) = await _authService.DeleteAccountAsync();
                if (!deleted)
                {
                    await Shell.Current.DisplayAlertAsync(L["ErrorTitle"], error, "OK");
                    return;
                }

                Preferences.Default.Clear();

                var loginPage = Application.Current.Handler.MauiContext
                    .Services.GetService<Views.LoginPage>();
                if (Application.Current?.Windows.Count > 0)
                    Application.Current.Windows[0].Page = loginPage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProfileVM] DeleteAccount error: {ex.Message}");
                await Shell.Current.DisplayAlertAsync(L["ErrorTitle"], ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
