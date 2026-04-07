using Microsoft.Extensions.DependencyInjection;

namespace BillWise
{
    public partial class App : Application
    {
        private readonly Models.Services.AuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        public App(Models.Services.AuthService authService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _authService = authService;
            _serviceProvider = serviceProvider;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            if (_authService.IsUserLoggedIn())
            {
                return new Window(new AppShell());
            }
            else
            {
                var loginPage = _serviceProvider.GetService<Views.LoginPage>();
                if (loginPage != null)
                {
                    return new Window(new NavigationPage(loginPage));
                }
                
                // Fallback to Shell if LoginPage for some reason fails to resolve
                return new Window(new AppShell());
            }
        }
    }
}