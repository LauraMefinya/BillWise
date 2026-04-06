using System.Collections.ObjectModel;
using BillWise.Models.Entities;
using BillWise.Models.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillWise.ViewModels
{
    public partial class InvoicesViewModel : BaseViewModel
    {
        private readonly InvoiceService _invoiceService;

        public InvoicesViewModel(InvoiceService invoiceService)
        {
            Title = "My Invoices";
            _invoiceService = invoiceService;
        }

        public ObservableCollection<Invoice> Invoices { get; } = new();

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private int _allCount;

        [ObservableProperty]
        private int _paidCount;

        [ObservableProperty]
        private int _pendingCount;

        [ObservableProperty]
        private int _overdueCount;

        [RelayCommand]
        public async Task GoToDetailsAsync(Invoice invoice)
        {
            if (invoice == null) return;
            var parameters = new Dictionary<string, object> { { "Invoice", invoice } };
            await Shell.Current.GoToAsync(nameof(Views.InvoiceDetailsPage), parameters);
        }

        [RelayCommand]
        public async Task LoadDataAsync(string filter = "All")
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                List<Invoice> result;
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    result = await _invoiceService.SearchInvoicesAsync(SearchQuery);
                }
                else
                {
                    InvoiceStatus? status = filter switch
                    {
                        "Paid" => InvoiceStatus.Paid,
                        "Pending" => InvoiceStatus.Pending,
                        "Overdue" => InvoiceStatus.Overdue,
                        _ => null
                    };
                    result = await _invoiceService.GetInvoicesByStatusAsync(status);
                }

                Invoices.Clear();
                foreach (var inv in result)
                {
                    Invoices.Add(inv);
                }

                if (string.IsNullOrWhiteSpace(SearchQuery) && filter == "All")
                {
                    AllCount = result.Count;
                    PaidCount = result.Count(x => x.Status == InvoiceStatus.Paid);
                    PendingCount = result.Count(x => x.Status == InvoiceStatus.Pending);
                    OverdueCount = result.Count(x => x.Status == InvoiceStatus.Overdue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
