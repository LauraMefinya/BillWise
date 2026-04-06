using BillWise.Models.Entities;
using BillWise.Models.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillWise.ViewModels
{
    [QueryProperty(nameof(Invoice), "Invoice")]
    public partial class EditInvoiceViewModel : BaseViewModel
    {
        private readonly InvoiceService _invoiceService;

        public EditInvoiceViewModel(InvoiceService invoiceService)
        {
            Title = "Edit Invoice";
            _invoiceService = invoiceService;
        }

        private Invoice? _invoice;
        public Invoice? Invoice 
        {
            get => _invoice;
            set 
            {
                if (SetProperty(ref _invoice, value) && _invoice != null)
                {
                    LoadInvoiceData();
                }
            }
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
        private bool _isDateSelected = true;

        public string DueDateText => IsDateSelected ? DueDate.ToString("dd / MM / yyyy") : "jj / mm / aaaa";
        public Color DueDateColor => IsDateSelected ? Colors.Black : Color.FromArgb("#9CA3AF");

        [ObservableProperty]
        private CategoryType _selectedCategory = CategoryType.Other;

        [ObservableProperty]
        private string _paymentMethod = "Cash";

        [ObservableProperty]
        private string _notes = string.Empty;

        private void LoadInvoiceData()
        {
            InvoiceName = Invoice.Name;
            AmountText = Invoice.Amount.ToString();
            DueDate = Invoice.DueDate;
            SelectedCategory = Invoice.Category;
            PaymentMethod = Invoice.PaymentMethod.ToString() == "MobileMoney" ? "Mobile Money" : Invoice.PaymentMethod.ToString() == "BankTransfer" ? "Bank Transfer" : "Cash";
            Notes = Invoice.Notes ?? string.Empty;
        }

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

            if (Invoice == null) return;

            if (string.IsNullOrWhiteSpace(InvoiceName) || string.IsNullOrWhiteSpace(AmountText))
            {
                await Shell.Current.DisplayAlertAsync("Error", "Please fill Name and Amount fields.", "OK");
                return;
            }

            if (!decimal.TryParse(AmountText, out decimal amount))
            {
                await Shell.Current.DisplayAlertAsync("Error", "Invalid amount.", "OK");
                return; 
            }

            try
            {
                IsBusy = true;

                // Update the existing invoice object
                Invoice.Name = InvoiceName;
                Invoice.Amount = amount;
                Invoice.DueDate = DueDate;
                Invoice.Category = SelectedCategory;
                Invoice.Notes = Notes;
                Invoice.PaymentMethod = Enum.TryParse<Models.Entities.PaymentMethod>(PaymentMethod?.Replace(" ", ""), true, out var pm) ? pm : Models.Entities.PaymentMethod.Cash;
                
                var success = await _invoiceService.UpdateInvoiceAsync(Invoice);
                if (success)
                {
                    // Pass the updated invoice back so the detail page refreshes
                    var parameters = new Dictionary<string, object> { { "Invoice", Invoice } };
                    await Shell.Current.GoToAsync("..", parameters);
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Error", "Failed to update invoice in Supabase. Check your connection or tables.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
