using BillWise.Models.Entities;
using BillWise.Models.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillWise.ViewModels
{
    public partial class AddInvoiceViewModel : BaseViewModel
    {
        private readonly InvoiceService _invoiceService;

        public AddInvoiceViewModel(InvoiceService invoiceService)
        {
            Title = "New Invoice";
            _invoiceService = invoiceService;
            DueDate = DateTime.Today;
        }

        [ObservableProperty]
        private string _invoiceName = string.Empty;

        [ObservableProperty]
        private string _amountText = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DueDateText))]
        [NotifyPropertyChangedFor(nameof(DueDateColor))]
        private DateTime _dueDate = DateTime.Today;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DueDateText))]
        [NotifyPropertyChangedFor(nameof(DueDateColor))]
        private bool _isDateSelected = false;

        public string DueDateText => IsDateSelected ? DueDate.ToString("dd / MM / yyyy") : "jj / mm / aaaa";
        // Hex values directly converted or we can just return a string for ColorConverter. We can return Color.
        public Color DueDateColor => IsDateSelected ? Colors.Black : Color.FromArgb("#9CA3AF");

        [ObservableProperty]
        private CategoryType _selectedCategory = CategoryType.Other;

        [ObservableProperty]
        private string _paymentMethod = "Cash";

        [ObservableProperty]
        private string _notes = string.Empty;

        [RelayCommand]
        public void SelectCategory(CategoryType category)
        {
            SelectedCategory = category;
        }

        [RelayCommand]
        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task SaveInvoiceAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(InvoiceName) || string.IsNullOrWhiteSpace(AmountText))
            {
                await Shell.Current.DisplayAlert("Error", "Please fill Name and Amount fields.", "OK");
                return;
            }

            if (!decimal.TryParse(AmountText, out decimal amount))
            {
                await Shell.Current.DisplayAlert("Error", "Invalid amount.", "OK");
                return; 
            }

            try
            {
                IsBusy = true;

                var invoice = new Invoice
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = InvoiceName,
                    Amount = amount,
                    DueDate = DueDate,
                    Category = SelectedCategory,
                    Notes = Notes,
                    PaymentMethod = Enum.TryParse<Models.Entities.PaymentMethod>(PaymentMethod?.Replace(" ", ""), true, out var pm) ? pm : Models.Entities.PaymentMethod.Cash,
                    Status = InvoiceStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                // Default payment method setup if it's already marked as paid, but default is Pending.
                
                var success = await _invoiceService.AddInvoiceAsync(invoice);
                if (success)
                {
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to save invoice to Supabase. Check your connection or tables.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
