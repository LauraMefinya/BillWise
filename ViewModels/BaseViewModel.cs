using BillWise.Models.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace BillWise.ViewModels
{
    public partial class BaseViewModel : ObservableObject,
        IRecipient<LanguageChangedMessage>
    {
        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private bool _isBusy = false;

        protected BaseViewModel()
        {
            // Register to receive language change messages
            WeakReferenceMessenger.Default.Register(this);
        }

        // Called when language changes — refresh all bindings
        public virtual void Receive(LanguageChangedMessage message)
        {
            OnPropertyChanged(string.Empty);
        }
    }
}