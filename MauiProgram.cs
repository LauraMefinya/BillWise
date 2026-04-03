using BillWise.Models.Services;
using BillWise.ViewModels;
using BillWise.Views;
using Microsoft.Extensions.Logging;
using Supabase;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace BillWise
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // SUPABASE CONFIGURATION
            var url = "https://xnlstfhdhghhhmhmdhgm.supabase.co";
            var key = "sb_publishable_PkUeHm6SLzHVXu5B0pYiEA_sR1mL6Ux";
            var options = new SupabaseOptions { AutoConnectRealtime = true };
            builder.Services.AddSingleton(new Supabase.Client(url, key, options));

            // Services
            builder.Services.AddSingleton<InvoiceService>();

            // ViewModels
            builder.Services.AddSingleton<HomeViewModel>();
            builder.Services.AddSingleton<InvoicesViewModel>();
            builder.Services.AddSingleton<StatisticsViewModel>();
            builder.Services.AddSingleton<AlertsViewModel>();
            builder.Services.AddTransient<AddInvoiceViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();

            // Pages
            builder.Services.AddSingleton<HomePage>();
            builder.Services.AddSingleton<InvoicesPage>();
            builder.Services.AddSingleton<StatisticsPage>();
            builder.Services.AddSingleton<AlertsPage>();
            builder.Services.AddTransient<AddInvoicePage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<EditProfilePage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // Set initial language
            var savedLang = Preferences.Default.Get("language", "en");
            try
            {
                BillWise.Resources.Strings.LocalizationResourceManager.Instance.SetCulture(new System.Globalization.CultureInfo(savedLang));
            }
            catch {}

            return app;
        }
    }
}