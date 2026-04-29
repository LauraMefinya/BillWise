using BillWise.Models.Messages;
using BillWise.Models.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace BillWise.ViewModels
{
    public partial class BaseViewModel : ObservableObject,
        IRecipient<LanguageChangedMessage>,
        IRecipient<CurrencyChangedMessage>
    {
        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private bool _isBusy = false;

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

        // Called when language changes — refresh all bindings
        public virtual void Receive(LanguageChangedMessage message)
        {
            OnPropertyChanged(string.Empty);
        }

        // Called when currency changes — refresh CurrencySymbol
        public virtual void Receive(CurrencyChangedMessage message)
        {
            OnPropertyChanged(nameof(CurrencySymbol));
        }
    }
}