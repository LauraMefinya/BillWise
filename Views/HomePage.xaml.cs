using BillWise.ViewModels;

namespace BillWise.Views
{
    public partial class HomePage : ContentPage
    {
        private readonly HomeViewModel _viewModel;

        public HomePage(HomeViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadDataAsync();
            StartShakeDetection();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopShakeDetection();
        }

        private void StartShakeDetection()
        {
            if (!Preferences.Default.Get("shake_to_add", true)) return;
            if (!Accelerometer.Default.IsSupported) return;
            if (Accelerometer.Default.IsMonitoring) return;

            Accelerometer.Default.ShakeDetected += OnShakeDetected;
            Accelerometer.Default.Start(SensorSpeed.Game);
        }

        private void StopShakeDetection()
        {
            if (!Accelerometer.Default.IsMonitoring) return;
            Accelerometer.Default.ShakeDetected -= OnShakeDetected;
            Accelerometer.Default.Stop();
        }

        private async void OnShakeDetected(object? sender, EventArgs e)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                TriggerHaptic(HapticFeedbackType.LongPress);
                await Shell.Current.GoToAsync(nameof(AddInvoicePage));
            });
        }

        private async void OnFabClicked(object sender, EventArgs e)
        {
            try
            {
                TriggerHaptic(HapticFeedbackType.Click);
                await Shell.Current.GoToAsync(nameof(AddInvoicePage));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Navigation Error", $"Failed to open page: {ex.Message}", "OK");
            }
        }

        private static void TriggerHaptic(HapticFeedbackType type = HapticFeedbackType.Click)
        {
            if (!Preferences.Default.Get("haptic_enabled", true)) return;
            if (HapticFeedback.Default.IsSupported)
                HapticFeedback.Default.Perform(type);
        }
    }
}