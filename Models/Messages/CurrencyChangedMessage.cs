using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BillWise.Models.Messages
{
    public class CurrencyChangedMessage : ValueChangedMessage<string>
    {
        public CurrencyChangedMessage(string currency) : base(currency) { }
    }
}
