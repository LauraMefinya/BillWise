using Microsoft.Extensions.DependencyInjection;

namespace BillWise
{
    public partial class App : Application
    {
        private readonly Models.Services.AuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        public App(Models.Services.AuthService authService,
                   IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _authService = authService;
            _serviceProvider = serviceProvider;
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
            }
            else
            {
                // No session — go to login
                var loginPage = _serviceProvider.GetService<Views.LoginPage>();
                if (Windows.Count > 0)
                    Windows[0].Page = new NavigationPage(loginPage);
            }
        }
    }
}