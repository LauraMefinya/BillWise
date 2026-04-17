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

        [ObservableProperty]
        private string _selectedFilter = "All";

        partial void OnSelectedFilterChanged(string value)
        {
            OnPropertyChanged(nameof(AllChipBackground));
            OnPropertyChanged(nameof(AllChipTextColor));
            OnPropertyChanged(nameof(PaidChipBackground));
            OnPropertyChanged(nameof(PaidChipTextColor));
            OnPropertyChanged(nameof(PendingChipBackground));
            OnPropertyChanged(nameof(PendingChipTextColor));
            OnPropertyChanged(nameof(OverdueChipBackground));
            OnPropertyChanged(nameof(OverdueChipTextColor));
        }

        partial void OnSearchQueryChanged(string value)
        {
            _ = LoadDataAsync(SelectedFilter);
        }

        private static Color InactiveChipBg =>
            Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#374151") : Color.FromArgb("#F3F4F6");
        private static Color InactiveChipText =>
            Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#D1D5DB") : Color.FromArgb("#4B5563");

        public Color AllChipBackground     => SelectedFilter == "All"     ? Color.FromArgb("#2196F3") : InactiveChipBg;
        public Color AllChipTextColor      => SelectedFilter == "All"     ? Colors.White : InactiveChipText;
        public Color PaidChipBackground    => SelectedFilter == "Paid"    ? Color.FromArgb("#2196F3") : InactiveChipBg;
        public Color PaidChipTextColor     => SelectedFilter == "Paid"    ? Colors.White : InactiveChipText;
        public Color PendingChipBackground => SelectedFilter == "Pending" ? Color.FromArgb("#2196F3") : InactiveChipBg;
        public Color PendingChipTextColor  => SelectedFilter == "Pending" ? Colors.White : InactiveChipText;
        public Color OverdueChipBackground => SelectedFilter == "Overdue" ? Color.FromArgb("#2196F3") : InactiveChipBg;
        public Color OverdueChipTextColor  => SelectedFilter == "Overdue" ? Colors.White : InactiveChipText;

        public override void Receive(Models.Messages.CurrencyChangedMessage message)
        {
            base.Receive(message);
            if (!IsBusy)
                LoadDataCommand.Execute(null);
        }

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
                SelectedFilter = filter;

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
