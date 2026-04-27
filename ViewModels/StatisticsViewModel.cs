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
using LiveChartsCore.Measure;
using SkiaSharp;

namespace BillWise.ViewModels
{
    public partial class StatisticsViewModel : BaseViewModel
    {
        private readonly InvoiceService _invoiceService;

        public StatisticsViewModel(InvoiceService invoiceService)
        {
            Title = "Statistics";
            _invoiceService = invoiceService;
        }

        [ObservableProperty]
        private decimal _totalAmount;

        [ObservableProperty]
        private decimal _thisMonthAmount;

        [ObservableProperty]
        private decimal _paidAmount;

        [ObservableProperty]
        private decimal _toPayAmount;

        public string FormattedTotalAmount     => CurrencyService.Format(TotalAmount);
        public string FormattedThisMonthAmount => CurrencyService.Format(ThisMonthAmount);
        public string FormattedPaidAmount      => CurrencyService.Format(PaidAmount);
        public string FormattedToPayAmount     => CurrencyService.Format(ToPayAmount);

        partial void OnTotalAmountChanged(decimal value)     => OnPropertyChanged(nameof(FormattedTotalAmount));
        partial void OnThisMonthAmountChanged(decimal value) => OnPropertyChanged(nameof(FormattedThisMonthAmount));
        partial void OnPaidAmountChanged(decimal value)      => OnPropertyChanged(nameof(FormattedPaidAmount));
        partial void OnToPayAmountChanged(decimal value)     => OnPropertyChanged(nameof(FormattedToPayAmount));

        [ObservableProperty]
        private ObservableCollection<ISeries> _barSeries = new();

        [ObservableProperty]
        private Axis[] _xAxes = new[] { new Axis() };

        [ObservableProperty]
        private Axis[] _yAxes = new[] { new Axis() };

        [ObservableProperty]
        private ObservableCollection<ISeries> _pieSeries = new();

        public ObservableCollection<CategoryStat> CategoryStats { get; } = new();

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                TotalAmount = await _invoiceService.GetTotalAmountAsync();
                PaidAmount = await _invoiceService.GetTotalPaidAsync();
                ToPayAmount = await _invoiceService.GetTotalToPayAsync();

                var monthly = await _invoiceService.GetMonthlyExpensesAsync();
                if (monthly.Count > 0)
                {
                    ThisMonthAmount = monthly.Last().Amount;

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

                var categories = await _invoiceService.GetExpensesByCategoryAsync();
                PieSeries.Clear();
                CategoryStats.Clear();

                var total = categories.Values.Sum();

                foreach (var cat in categories)
                {
                    var color = GetCategoryColor(cat.Key);
                    PieSeries.Add(new PieSeries<double>
                    {
                        Values = new double[] { (double)CurrencyService.Convert(cat.Value) },
                        Name = cat.Key.ToString(),
                        Fill = new SolidColorPaint(color)
                    });

                    var percentage = total > 0 ? (double)cat.Value / (double)total : 0;
                    var hex = $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
                    CategoryStats.Add(new CategoryStat
                    {
                        CategoryName = LocalizationResourceManager.Instance[cat.Key.ToString()],
                        AmountFormatted = CurrencyService.Format(cat.Value),
                        ColorHex = hex,
                        BarWidth = (int)(percentage * 220)
                    });
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

        public override void Receive(CurrencyChangedMessage message)
        {
            base.Receive(message);
            OnPropertyChanged(nameof(FormattedTotalAmount));
            OnPropertyChanged(nameof(FormattedThisMonthAmount));
            OnPropertyChanged(nameof(FormattedPaidAmount));
            OnPropertyChanged(nameof(FormattedToPayAmount));
            if (!IsBusy)
                LoadDataCommand.Execute(null);
        }

        private SKColor GetCategoryColor(CategoryType type)
        {
            return type switch
            {
                CategoryType.Electricity => SKColors.Orange,
                CategoryType.Water => SKColors.DeepSkyBlue,
                CategoryType.Internet => SKColors.Purple,
                CategoryType.Rent => SKColors.Red,
                CategoryType.Subscription => SKColors.MediumSeaGreen,
                _ => SKColors.Gray
            };
        }
    }

    public class CategoryStat
    {
        public string CategoryName { get; set; } = string.Empty;
        public string AmountFormatted { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#000000";
        public int BarWidth { get; set; } = 0;
    }
}
