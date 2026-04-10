using CommunityToolkit.Mvvm.Messaging;
using BillWise.Models.Messages;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace BillWise.Resources.Strings
{
    public class LocalizationResourceManager : INotifyPropertyChanged
    {
        private static readonly LocalizationResourceManager _instance = new();
        public static LocalizationResourceManager Instance => _instance;

        private readonly ResourceManager _resourceManager;

        private LocalizationResourceManager()
        {
            _resourceManager = new ResourceManager(
                "BillWise.Resources.Strings.AppResources",
                typeof(LocalizationResourceManager).Assembly);
        }

        public string this[string text]
        {
            get
            {
                var val = _resourceManager.GetString(text, CultureInfo.CurrentUICulture);
                return val ?? text;
            }
        }

        public void SetCulture(CultureInfo culture)
        {
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;

            // Notify all bindings to refresh (null = all properties, Item[] = all indexer bindings)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));

            // Broadcast to all ViewModels
            WeakReferenceMessenger.Default.Send(
                new LanguageChangedMessage(culture.Name));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}