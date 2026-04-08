using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BillWise.Models.Messages
{
    // Message broadcast when user changes language
    public class LanguageChangedMessage : ValueChangedMessage<string>
    {
        public LanguageChangedMessage(string language) : base(language) { }
    }
}