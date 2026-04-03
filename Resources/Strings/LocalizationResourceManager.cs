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
            // Update this if the namespace differs
            _resourceManager = new ResourceManager("BillWise.Resources.Strings.AppResources", typeof(LocalizationResourceManager).Assembly);
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null)); // Refresh all bindings
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
