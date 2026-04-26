using BillWise.Models.Entities;
using BillWise.Models.Services;
using BillWise.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillWise.ViewModels
{
    [QueryProperty(nameof(Invoice), "Invoice")]
    public partial class InvoiceDetailsViewModel : BaseViewModel
    {
        private readonly InvoiceService _invoiceService;

        public InvoiceDetailsViewModel(InvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPaid))]
        [NotifyPropertyChangedFor(nameof(ShowPaymentAction))]
        [NotifyPropertyChangedFor(nameof(ShowEditAction))]
        [NotifyPropertyChangedFor(nameof(StatusBadgeBackgroundColor))]
        [NotifyPropertyChangedFor(nameof(StatusBadgeTextColor))]
        [NotifyPropertyChangedFor(nameof(StatusText))]
        [NotifyPropertyChangedFor(nameof(PaymentDateText))]
        private Invoice? _invoice;

        public bool IsPaid => Invoice?.Status == InvoiceStatus.Paid;
        public bool ShowPaymentAction => Invoice != null && Invoice.Status != InvoiceStatus.Paid;
        public bool ShowEditAction => Invoice != null && Invoice.Status != InvoiceStatus.Paid;

        // Uses resource keys so the text changes with the app language
        public string StatusText => Invoice?.Status switch
        {
            InvoiceStatus.Paid    => LocalizationResourceManager.Instance["Paid"],
            InvoiceStatus.Overdue => LocalizationResourceManager.Instance["Overdue"],
            _                     => LocalizationResourceManager.Instance["Pending"]
        };

        public string StatusBadgeBackgroundColor => Invoice?.Status switch
        {
            InvoiceStatus.Paid    => "#D1FAE5",   // Light Green
            InvoiceStatus.Overdue => "#FEE2E2",   // Light Red
            _                     => "#FEF3C7"    // Light Yellow
        };

        public string StatusBadgeTextColor => Invoice?.Status switch
        {
            InvoiceStatus.Paid    => "#065F46",   // Dark Green
            InvoiceStatus.Overdue => "#B91C1C",   // Dark Red
            _                     => "#92400E"    // Dark Yellow
        };

        public string PaymentDateText => Invoice?.PaidAt?.ToString("dd MMMM yyyy") ?? "-";

        public override void Receive(Models.Messages.CurrencyChangedMessage message)
        {
            base.Receive(message);
            OnPropertyChanged(nameof(Invoice));
        }

        [RelayCommand]
        public async Task GoBackAsync() => await Shell.Current.GoToAsync("..");

        [RelayCommand]
        public async Task DeleteInvoiceAsync()
        {
            var L = LocalizationResourceManager.Instance;
            if (Invoice == null) return;

            bool confirm = await Shell.Current.DisplayAlertAsync(
                L["WarningTitle"],
                string.Format(L["ConfirmDeleteInvoice"], Invoice.Name),
                L["Yes"], L["No"]);

            if (confirm)
            {
                IsBusy = true;
                bool success = await _invoiceService.DeleteInvoiceAsync(Invoice.Id);
                IsBusy = false;

                if (success)
                {
                    TriggerHaptic(HapticFeedbackType.Click);
                    await Shell.Current.GoToAsync("..");
                }
                else
                    await Shell.Current.DisplayAlertAsync(L["ErrorTitle"], L["FailedDeleteInvoice"], "OK");
            }
        }

        [RelayCommand]
        public async Task EditInvoiceAsync()
        {
            if (Invoice == null) return;
            var parameters = new Dictionary<string, object> { { "Invoice", Invoice } };
            await Shell.Current.GoToAsync(nameof(Views.EditInvoicePage), parameters);
        }

        [RelayCommand]
        public async Task MarkAsPaidAsync()
        {
            var L = LocalizationResourceManager.Instance;
            if (Invoice == null) return;

            var optBank   = L["PaymentBankTransfer"];
            var optPayPal = L["PaymentPayPal"];
            var optGoogle = L["PaymentGooglePay"];
            var optCash   = L["PaymentCash"];
            var optCard   = L["PaymentCardPayment"];
            var optDebit  = L["PaymentDirectDebit"];

            string action = await Shell.Current.DisplayActionSheetAsync(
                L["SelectPaymentMethodTitle"], L["Cancel"], null,
                optBank, optPayPal, optGoogle, optCash, optCard, optDebit);

            if (string.IsNullOrEmpty(action) || action == L["Cancel"]) return;

            PaymentMethod method;
            if (action == optBank)        method = PaymentMethod.BankTransfer;
            else if (action == optPayPal) method = PaymentMethod.PayPal;
            else if (action == optGoogle) method = PaymentMethod.GooglePay;
            else if (action == optCash)   method = PaymentMethod.Cash;
            else if (action == optCard)   method = PaymentMethod.CardPayment;
            else if (action == optDebit)  method = PaymentMethod.DirectDebit;
            else return;

            Invoice.MarkAsPaid(method);

            IsBusy = true;
            bool success = await _invoiceService.UpdateInvoiceAsync(Invoice);
            IsBusy = false;

            if (success)
            {
                TriggerHaptic(HapticFeedbackType.LongPress);
                OnPropertyChanged(nameof(IsPaid));
                OnPropertyChanged(nameof(ShowPaymentAction));
                OnPropertyChanged(nameof(ShowEditAction));
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(StatusBadgeBackgroundColor));
                OnPropertyChanged(nameof(StatusBadgeTextColor));
                OnPropertyChanged(nameof(PaymentDateText));
            }
            else
                await Shell.Current.DisplayAlertAsync(L["ErrorTitle"], L["FailedUpdateInvoice"], "OK");
        }

        private static void TriggerHaptic(HapticFeedbackType type = HapticFeedbackType.Click)
        {
            if (!Preferences.Default.Get("haptic_enabled", true)) return;
            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(type == HapticFeedbackType.LongPress ? 150 : 60));
        }
    }
}
