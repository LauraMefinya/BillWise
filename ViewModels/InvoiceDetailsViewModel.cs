using BillWise.Models.Entities;
using BillWise.Models.Services;
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

        public string StatusText => Invoice?.Status switch
        {
            InvoiceStatus.Paid => "Paid",
            InvoiceStatus.Overdue => "Overdue",
            _ => "Pending"
        };

        public string StatusBadgeBackgroundColor => Invoice?.Status switch
        {
            InvoiceStatus.Paid => "#D1FAE5",      // Light Green
            InvoiceStatus.Overdue => "#FEE2E2",   // Light Red
            _ => "#FEF3C7"                        // Light Yellow
        };

        public string StatusBadgeTextColor => Invoice?.Status switch
        {
            InvoiceStatus.Paid => "#065F46",      // Dark Green
            InvoiceStatus.Overdue => "#B91C1C",   // Dark Red
            _ => "#92400E"                        // Dark Yellow (Brown)
        };

        public string PaymentDateText => Invoice?.PaidAt?.ToString("dd MMMM yyyy") ?? "-";

        [RelayCommand]
        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task DeleteInvoiceAsync()
        {
            if (Invoice == null) return;

            bool confirm = await Shell.Current.DisplayAlertAsync("Delete Invoice", $"Are you sure you want to delete {Invoice.Name}?", "Yes", "No");
            if (confirm)
            {
                IsBusy = true;
                bool success = await _invoiceService.DeleteInvoiceAsync(Invoice.Id);
                IsBusy = false;

                if (success)
                {
                    await Shell.Current.GoToAsync(".."); // Go back to list
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Error", "Failed to delete the invoice.", "OK");
                }
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
            if (Invoice == null) return;
            
            // In a real flow, this might open a bottom sheet to select payment method.
            // Using a simple action sheet for now.
            string action = await Shell.Current.DisplayActionSheetAsync("Select Payment Method", "Cancel", null, "Cash", "Mobile Money", "Bank Transfer");
            
            if (action == "Cancel" || string.IsNullOrEmpty(action)) return;

            if (Enum.TryParse<PaymentMethod>(action.Replace(" ", ""), true, out var method))
            {
                Invoice.MarkAsPaid(method);
                
                IsBusy = true;
                bool success = await _invoiceService.UpdateInvoiceAsync(Invoice);
                IsBusy = false;

                if (success)
                {
                    // Update property to trigger UI refresh
                    OnPropertyChanged(nameof(Invoice)); 
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Error", "Failed to update the invoice status.", "OK");
                }
            }
        }
    }
}
