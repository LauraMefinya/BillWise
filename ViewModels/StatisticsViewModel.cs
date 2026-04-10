using System.Collections.ObjectModel;
using BillWise.Models.Entities;
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

        [ObservableProperty]
        private ObservableCollection<ISeries> _barSeries = new();

        [ObservableProperty]
        private Axis[] _xAxes = new[] { new Axis() };

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

                    var values = monthly.Select(m => (double)m.Amount).ToArray();
                    var labels = monthly.Select(m => m.Month.ToString("MMM")).ToArray();

                    BarSeries = new ObservableCollection<ISeries>
                    {
                        new ColumnSeries<double>
                        {
                            Values = values,
                            Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                            MaxBarWidth = 30
                        }
                    };

                    XAxes = new Axis[]
                    {
                        new Axis
                        {
                            Labels = labels,
                            TextSize = 12,
                            LabelsPaint = new SolidColorPaint(SKColors.Gray)
                        }
                    };
                }

                var categories = await _invoiceService.GetExpensesByCategoryAsync();
                PieSeries.Clear();
                CategoryStats.Clear();

                foreach (var cat in categories)
                {
                    var color = GetCategoryColor(cat.Key);
                    PieSeries.Add(new PieSeries<double>
                    {
                        Values = new double[] { (double)cat.Value },
                        Name = cat.Key.ToString(),
                        Fill = new SolidColorPaint(color),
                        InnerRadius = 60
                    });

                    CategoryStats.Add(new CategoryStat
                    {
                        CategoryName = LocalizationResourceManager.Instance[cat.Key.ToString()],
                        AmountFormatted = $"{cat.Value:N0} FCFA",
                        ColorHex = "#" + color.ToString().Substring(2)
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
    }
}
