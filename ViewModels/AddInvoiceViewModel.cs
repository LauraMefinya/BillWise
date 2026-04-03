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
        private string _paymentMethod = "Cash";

        [ObservableProperty]
        private string _notes = string.Empty;

        [RelayCommand]
        public void SelectCategory(CategoryType category)
        {
            SelectedCategory = category;
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
