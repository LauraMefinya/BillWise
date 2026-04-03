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

        public ObservableCollection<Invoice> UpcomingInvoices { get; } = new();

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
                UpcomingInvoices.Clear();
                foreach (var inv in upcoming)
                {
                    UpcomingInvoices.Add(inv);
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
