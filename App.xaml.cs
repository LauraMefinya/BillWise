using BillWise.Resources.Strings;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace BillWise
{
    public partial class App : Application
    {
        private readonly Models.Services.AuthService _authService;
        private readonly Models.Services.LocalNotificationScheduler _scheduler;
        private readonly IServiceProvider _serviceProvider;

        public App(Models.Services.AuthService authService,
                   Models.Services.LocalNotificationScheduler scheduler,
                   IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _authService = authService;
            _scheduler = scheduler;
            _serviceProvider = serviceProvider;
            RestoreLanguage();
            RestoreTheme();
        }

        private static void RestoreTheme()
        {
            Current!.UserAppTheme = Preferences.Default.Get("dark_mode", false)
                ? AppTheme.Dark
                : AppTheme.Light;
        }

        private static void RestoreLanguage()
        {
            var saved = Preferences.Default.Get("language", string.Empty);
            if (!string.IsNullOrEmpty(saved))
            {
                try
                {
                    LocalizationResourceManager.Instance.SetCulture(
                        new CultureInfo(saved));
                }
                catch { }
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Show a loading page while we restore session
            return new Window(new ContentPage
            {
                BackgroundColor = Color.FromArgb("#3498DB"),
                Content = new ActivityIndicator
                {
                    IsRunning = true,
                    Color = Colors.White,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            });
        }

        protected override async void OnStart()
        {
            base.OnStart();
            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            // Try to restore saved session
            var restored = await _authService.RestoreSessionAsync();

            if (restored || _authService.IsUserLoggedIn())
            {
                // Session valid — go to main app
                if (Windows.Count > 0)
                    Windows[0].Page = new AppShell();

                // Schedule OS-level notifications in the background
                _ = _scheduler.ScheduleAsync();
            }
            else
            {
                // No session — go to login
                var loginPage = _serviceProvider.GetService<Views.LoginPage>();
                if (Windows.Count > 0)
                    Windows[0].Page = new NavigationPage(loginPage);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (_authService.IsUserLoggedIn())
                _ = _scheduler.ScheduleAsync();
        }
    }
}