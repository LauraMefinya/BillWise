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
                    await Shell.Current.GoToAsync("..");
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

            string action = await Shell.Current.DisplayActionSheetAsync(
                L["SelectPaymentMethodTitle"], L["Cancel"], null,
                optBank, optPayPal, optGoogle);

            if (string.IsNullOrEmpty(action) || action == L["Cancel"]) return;

            PaymentMethod method;
            if (action == optBank)        method = PaymentMethod.BankTransfer;
            else if (action == optPayPal) method = PaymentMethod.PayPal;
            else if (action == optGoogle) method = PaymentMethod.GooglePay;
            else return;

            Invoice.MarkAsPaid(method);

            IsBusy = true;
            bool success = await _invoiceService.UpdateInvoiceAsync(Invoice);
            IsBusy = false;

            if (success)
            {
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
    }
}
