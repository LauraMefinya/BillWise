using BillWise.Models.Providers;
using BillWise.Models.Services;
using BillWise.ViewModels;
using BillWise.Views;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Media;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.Maui.OCR;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Supabase;

namespace BillWise
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, _) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("NoUnderline", (handler, _) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, _) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

            Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, _) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement(false)
                .UseSkiaSharp()
                .UseOcr()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // SUPABASE
            var url = "https://xnlstfhdhghhhmhmdhgm.supabase.co";
            var key = "sb_publishable_PkUeHm6SLzHVXu5B0pYiEA_sR1mL6Ux";
            var options = new SupabaseOptions { AutoConnectRealtime = true };
            builder.Services.AddSingleton(new Supabase.Client(url, key, options));

            // Services
            builder.Services.AddSingleton<ISpeechToText>(SpeechToText.Default);
            builder.Services.AddSingleton(OcrPlugin.Default);
            builder.Services.AddSingleton<SessionService>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<UserProfileService>();
            builder.Services.AddSingleton<InvoiceService>();
            builder.Services.AddSingleton<NotificationService>();
            builder.Services.AddSingleton<LocalNotificationScheduler>();
            builder.Services.AddSingleton<InvoiceProvider>();
            builder.Services.AddSingleton<PdfExportService>();

            // ViewModels
            builder.Services.AddSingleton<HomeViewModel>();
            builder.Services.AddSingleton<InvoicesViewModel>();
            builder.Services.AddSingleton<StatisticsViewModel>();
            builder.Services.AddSingleton<AlertsViewModel>();
            builder.Services.AddTransient<AddInvoiceViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<InvoiceDetailsViewModel>();
            builder.Services.AddTransient<EditInvoiceViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();

            // Pages
            builder.Services.AddSingleton<HomePage>();
            builder.Services.AddSingleton<InvoicesPage>();
            builder.Services.AddSingleton<StatisticsPage>();
            builder.Services.AddSingleton<AlertsPage>();
            builder.Services.AddTransient<AddInvoicePage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<EditProfilePage>();
            builder.Services.AddTransient<InvoiceDetailsPage>();
            builder.Services.AddTransient<EditInvoicePage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<AccountSettingsPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            var savedLang = Preferences.Default.Get("language", "en");
            try
            {
                BillWise.Resources.Strings.LocalizationResourceManager
                    .Instance.SetCulture(
                        new System.Globalization.CultureInfo(savedLang));
            }
            catch { }

            return app;
        }
    }
}