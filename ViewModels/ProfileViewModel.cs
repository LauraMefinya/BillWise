using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillWise.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        public ProfileViewModel()
        {
            Title = "Profile";
            LoadSettings();
        }

        [ObservableProperty]
        private string _userName = "User";

        [ObservableProperty]
        private string _userEmail = "user@example.com";

        [ObservableProperty]
        private int _totalInvoices = 0;

        [ObservableProperty]
        private int _paidInvoices = 0;

        [ObservableProperty]
        private int _pendingInvoices = 0;

        [ObservableProperty]
        private int _overdueInvoices = 0;

        // Settings 
        [ObservableProperty]
        private string _selectedCurrency = "FCFA";

        [ObservableProperty]
        private string _selectedLanguage = "en";

        [ObservableProperty]
        private bool _notificationsEnabled = true;

        [ObservableProperty]
        private int _reminderDays = 2;

        [ObservableProperty]
        private bool _hapticFeedbackEnabled = true;

        [ObservableProperty]
        private bool _shakeToAddEnabled = true;

        private void LoadSettings()
        {
            // Ideally we load this from settings service/preferences
            SelectedCurrency = Preferences.Default.Get("currency", "FCFA");
            SelectedLanguage = Preferences.Default.Get("language", "en");
            NotificationsEnabled = Preferences.Default.Get("notif_enabled", true);
            ReminderDays = Preferences.Default.Get("reminder_days", 2);
            HapticFeedbackEnabled = Preferences.Default.Get("haptic_enabled", true);
            ShakeToAddEnabled = Preferences.Default.Get("shake_to_add", true);
        }

        [RelayCommand]
        public void SaveSettings()
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
                BillWise.Resources.Strings.LocalizationResourceManager.Instance.SetCulture(culture);
            }
            catch { }

            Shell.Current.DisplayAlert("Success", "Settings saved.", "OK");
        }

        [RelayCommand]
        public void CancelChanges()
        {
            LoadSettings(); // Revert
        }

        [RelayCommand]
        public async Task ExportDataAsync()
        {
            await Shell.Current.DisplayAlert("Export", "Export feature coming soon.", "OK");
        }

        [RelayCommand]
        public async Task ClearAllDataAsync()
        {
            bool answer = await Shell.Current.DisplayAlert("Warning", "Are you sure you want to clear all local data? This cannot be undone.", "Yes", "No");
            if (answer)
            {
                Preferences.Default.Clear();
                await Shell.Current.DisplayAlert("Cleared", "All data cleared.", "OK");
            }
        }

        [RelayCommand]
        public async Task GoToEditProfileAsync()
        {
            await Shell.Current.GoToAsync(nameof(Views.EditProfilePage));
        }
    }
}
