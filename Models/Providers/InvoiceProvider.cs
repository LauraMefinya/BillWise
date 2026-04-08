using BillWise.Models.Entities;
using BillWise.Models.Services;

namespace BillWise.Models.Providers
{
    // Shared data provider — single source of truth for invoices across all pages
    public class InvoiceProvider
    {
        private readonly InvoiceService _invoiceService;

        public InvoiceProvider(InvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        // Cached invoice list
        private List<Invoice> _cachedInvoices = new();

        // Events to notify all subscribers when data changes
        public event Action? InvoicesChanged;

        // Load and cache invoices from Supabase
        public async Task<List<Invoice>> GetInvoicesAsync(bool forceRefresh = false)
        {
            if (forceRefresh || _cachedInvoices.Count == 0)
            {
                _cachedInvoices = await _invoiceService.GetAllInvoicesAsync();
            }
            return _cachedInvoices;
        }

        // Add invoice and notify all subscribers
        public async Task<bool> AddInvoiceAsync(Invoice invoice)
        {
            var success = await _invoiceService.AddInvoiceAsync(invoice);
            if (success)
            {
                _cachedInvoices = await _invoiceService.GetAllInvoicesAsync();
                InvoicesChanged?.Invoke();
            }
            return success;
        }

        // Update invoice and notify all subscribers
        public async Task<bool> UpdateInvoiceAsync(Invoice invoice)
        {
            var success = await _invoiceService.UpdateInvoiceAsync(invoice);
            if (success)
            {
                _cachedInvoices = await _invoiceService.GetAllInvoicesAsync();
                InvoicesChanged?.Invoke();
            }
            return success;
        }

        // Delete invoice and notify all subscribers
        public async Task<bool> DeleteInvoiceAsync(string id)
        {
            var success = await _invoiceService.DeleteInvoiceAsync(id);
            if (success)
            {
                _cachedInvoices.RemoveAll(i => i.Id == id);
                InvoicesChanged?.Invoke();
            }
            return success;
        }

        // Mark as paid and notify all subscribers
        public async Task<bool> MarkAsPaidAsync(string invoiceId, PaymentMethod method)
        {
            var success = await _invoiceService.MarkAsPaidAsync(invoiceId, method);
            if (success)
            {
                _cachedInvoices = await _invoiceService.GetAllInvoicesAsync();
                InvoicesChanged?.Invoke();
            }
            return success;
        }

        // Stats — computed from cache
        public int TotalCount => _cachedInvoices.Count;
        public int PaidCount => _cachedInvoices.Count(i => i.Status == InvoiceStatus.Paid);
        public int PendingCount => _cachedInvoices.Count(i => i.Status == InvoiceStatus.Pending);
        public int OverdueCount => _cachedInvoices.Count(i => i.Status == InvoiceStatus.Overdue);
        public decimal TotalToPay => _cachedInvoices.Where(i => i.Status != InvoiceStatus.Paid).Sum(i => i.Amount);
        public decimal TotalPaid => _cachedInvoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.Amount);

        public List<Invoice> GetUpcoming() =>
            _cachedInvoices.Where(i => i.Status == InvoiceStatus.Pending
                && i.DaysUntilDue >= 0 && i.DaysUntilDue <= 7)
                .OrderBy(i => i.DueDate).ToList();

        public List<Invoice> GetOverdue() =>
            _cachedInvoices.Where(i => i.Status == InvoiceStatus.Overdue).ToList();

        public List<Invoice> GetByStatus(InvoiceStatus? status) =>
            status == null ? _cachedInvoices.ToList()
            : _cachedInvoices.Where(i => i.Status == status).ToList();

        public List<Invoice> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return _cachedInvoices.ToList();
            var lower = query.ToLower();
            return _cachedInvoices.Where(i =>
                i.Name.ToLower().Contains(lower) ||
                i.Amount.ToString().Contains(lower)).ToList();
        }
    }
}