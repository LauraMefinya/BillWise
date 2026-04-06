using System.Collections.ObjectModel;
using BillWise.Models.Entities;
using BillWise.Models.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillWise.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly InvoiceService _invoiceService;

        public HomeViewModel(InvoiceService invoiceService)
        {
            Title = "Home";
            _invoiceService = invoiceService;
        }

        [ObservableProperty]
        private decimal _totalToPay;

        [ObservableProperty]
        private decimal _totalPaid;

        [ObservableProperty]
        private int _overdueCount;

        [ObservableProperty]
        private bool _hasOverdueInvoices;

        public ObservableCollection<Invoice> RecentInvoices { get; } = new();

        [RelayCommand]
        public async Task GoToDetailsAsync(Invoice invoice)
        {
            if (invoice == null) return;
            var parameters = new Dictionary<string, object> { { "Invoice", invoice } };
            await Shell.Current.GoToAsync(nameof(Views.InvoiceDetailsPage), parameters);
        }

        [RelayCommand]
        public async Task GoToInvoicesFallbackAsync()
        {
            await Shell.Current.GoToAsync("//InvoicesPage");
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                TotalToPay = await _invoiceService.GetTotalToPayAsync();
                TotalPaid = await _invoiceService.GetTotalPaidAsync();

                var upcoming = await _invoiceService.GetUpcomingInvoicesAsync();
                RecentInvoices.Clear();
                foreach (var inv in upcoming)
                {
                    RecentInvoices.Add(inv);
                }

                var overdue = await _invoiceService.GetOverdueInvoicesAsync();
                OverdueCount = overdue.Count;
                HasOverdueInvoices = OverdueCount > 0;
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
