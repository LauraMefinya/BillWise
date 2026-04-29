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
                TriggerVibration(200);
                await Shell.Current.GoToAsync(nameof(AddInvoicePage));
            });
        }

        private static void TriggerVibration(int milliseconds = 60)
        {
            if (!Preferences.Default.Get("haptic_enabled", true)) return;
            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(milliseconds));
        }
    }
}