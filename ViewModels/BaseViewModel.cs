using BillWise.Models.Messages;
using BillWise.Models.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace BillWise.ViewModels
{
    /// <summary>
    /// The base view model class providing common properties and messaging functionality 
    /// for all view models in the application.
    /// </summary>
    public partial class BaseViewModel : ObservableObject,
        IRecipient<LanguageChangedMessage>,
        IRecipient<CurrencyChangedMessage>
    {
        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private bool _isBusy = false;

        /// <summary>
        /// Gets the current currency symbol formatted globally.
        /// </summary>
        public string CurrencySymbol => CurrencyService.Symbol;

        public IList<string> PaymentMethodOptions { get; } = new List<string>
        {
            "Bank Transfer", "PayPal", "Google Pay", "Cash", "Card Payment", "Direct Debit"
        };

        protected BaseViewModel()
        {
            // Register to receive language and currency change messages
            WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this);
            WeakReferenceMessenger.Default.Register<CurrencyChangedMessage>(this);
        }

        /// <summary>
        /// Handles language change messages by triggering a UI refresh for localized strings.
        /// </summary>
        /// <param name="message">The language change message.</param>
        public virtual void Receive(LanguageChangedMessage message)
        {
            OnPropertyChanged(string.Empty);
        }

        /// <summary>
        /// Handles currency change messages by refreshing the CurrencySymbol property.
        /// </summary>
        /// <param name="message">The currency change message.</param>
        public virtual void Receive(CurrencyChangedMessage message)
        {
            OnPropertyChanged(nameof(CurrencySymbol));
        }
    }
}