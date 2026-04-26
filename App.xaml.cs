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
        private readonly Models.Services.UserProfileService _userProfileService;

        public App(Models.Services.AuthService authService,
                   Models.Services.LocalNotificationScheduler scheduler,
                   IServiceProvider serviceProvider,
                   Models.Services.UserProfileService userProfileService)
        {
            InitializeComponent();
            _authService = authService;
            _scheduler = scheduler;
            _serviceProvider = serviceProvider;
            _userProfileService = userProfileService;
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
            try
            {
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[App] OnStart crash: {ex}");
                GoToLoginFallback();
            }
        }

        private async Task InitializeAsync()
        {
            bool restored;
            try { restored = await _authService.RestoreSessionAsync(); }
            catch { restored = false; }

            if (restored || _authService.IsUserLoggedIn())
            {
                try
                {
                    var savedName = Preferences.Default.Get("user_name", string.Empty);
                    if (string.IsNullOrEmpty(savedName))
                    {
                        var userId = _authService.GetCurrentUserId();
                        if (!string.IsNullOrEmpty(userId))
                        {
                            var name = await _userProfileService.FetchNameAsync(userId);
                            if (!string.IsNullOrEmpty(name))
                                Preferences.Default.Set("user_name", name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[App] Profile fetch error: {ex.Message}");
                }

                if (Windows.Count > 0)
                    Windows[0].Page = new AppShell();

                _ = _scheduler.ScheduleAsync();
            }
            else
            {
                GoToLoginFallback();
            }
        }

        private void GoToLoginFallback()
        {
            try
            {
                var loginPage = _serviceProvider.GetService<Views.LoginPage>();
                if (Windows.Count > 0 && loginPage != null)
                    Windows[0].Page = loginPage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[App] Login fallback error: {ex}");
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