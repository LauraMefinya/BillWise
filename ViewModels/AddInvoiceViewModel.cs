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
        private DateTime _dueDate;

        [ObservableProperty]
        private CategoryType _selectedCategory = CategoryType.Other;

        [ObservableProperty]
        private string _paymentMethod = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [RelayCommand]
        public async Task SaveInvoiceAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(InvoiceName) || string.IsNullOrWhiteSpace(AmountText))
            {
                return; // Validation failed
            }

            if (!decimal.TryParse(AmountText, out decimal amount))
            {
                return; // Invalid amount
            }

            try
            {
                IsBusy = true;

                var invoice = new Invoice
                {
                    Name = InvoiceName,
                    Amount = amount,
                    DueDate = DueDate,
                    Category = SelectedCategory,
                    Notes = Notes,
                    Status = InvoiceStatus.Pending
                };

                await _invoiceService.AddInvoiceAsync(invoice);
                await Shell.Current.GoToAsync("..");
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
