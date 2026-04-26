using System.Collections.ObjectModel;
using BillWise.Models.Entities;
using BillWise.Models.Messages;
using BillWise.Models.Services;
using BillWise.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

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

        public string UserGreeting
        {
            get
            {
                var name = Preferences.Default.Get("user_name", string.Empty);
                return !string.IsNullOrEmpty(name)
                    ? $"{LocalizationResourceManager.Instance["Hello"]}, {name}"
                    : LocalizationResourceManager.Instance["HomeGreeting"];
            }
        }

        [ObservableProperty]
        private decimal _totalToPay;

        [ObservableProperty]
        private decimal _totalPaid;

        public string FormattedTotalToPay => CurrencyService.Format(TotalToPay);
        public string FormattedTotalPaid  => CurrencyService.Format(TotalPaid);

        partial void OnTotalToPayChanged(decimal value) => OnPropertyChanged(nameof(FormattedTotalToPay));
        partial void OnTotalPaidChanged(decimal value)  => OnPropertyChanged(nameof(FormattedTotalPaid));

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(OverdueMessage))]
        private int _overdueCount;

        [ObservableProperty]
        private bool _hasOverdueInvoices;

        [ObservableProperty]
        private ObservableCollection<ISeries> _barSeries = new();

        [ObservableProperty]
        private Axis[] _xAxes = new[] { new Axis() };

        [ObservableProperty]
        private Axis[] _yAxes = new[] { new Axis() };

        public string OverdueMessage =>
            string.Format(LocalizationResourceManager.Instance["OverdueCountMessage"], OverdueCount);

        public ObservableCollection<Invoice> RecentInvoices { get; } = new();

        public override void Receive(CurrencyChangedMessage message)
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
        public async Task GoToInvoicesFallbackAsync()
        {
            await Shell.Current.GoToAsync("//InvoicesPage");
        }

        [RelayCommand]
        public async Task GoToAlertsAsync()
        {
            await Shell.Current.GoToAsync(nameof(Views.AlertsPage));
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

                var monthly = await _invoiceService.GetMonthlyExpensesAsync();
                if (monthly.Count > 0)
                {
                    var values = monthly.Select(m => (double)CurrencyService.Convert(m.Amount)).ToArray();
                    var labels = monthly.Select(m => m.Month.ToString("MMM")).ToArray();
                    var symbol = CurrencyService.Symbol;

                    BarSeries = new ObservableCollection<ISeries>
                    {
                        new ColumnSeries<double>
                        {
                            Values = values,
                            Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                            MaxBarWidth = 35,
                            Rx = 4,
                            Ry = 4
                        }
                    };

                    XAxes = new Axis[]
                    {
                        new Axis
                        {
                            Labels = labels,
                            TextSize = 11,
                            LabelsPaint = new SolidColorPaint(SKColors.Gray),
                            SeparatorsPaint = null
                        }
                    };

                    YAxes = new Axis[]
                    {
                        new Axis
                        {
                            Labeler = value => $"{value:N0} {symbol}",
                            TextSize = 10,
                            LabelsPaint = new SolidColorPaint(SKColors.Gray),
                            SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(UserGreeting));
            }
        }
    }
}
