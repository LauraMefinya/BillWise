using BillWise.Models.Entities;
using Supabase;

namespace BillWise.Models.Services
{
    public class InvoiceService
    {
        private readonly Supabase.Client _client;

        public InvoiceService(Supabase.Client client)
        {
            _client = client;
        }

        // ── READ ──────────────────────────────────────
        
        public async Task<List<Invoice>> GetAllInvoicesAsync()
        {
            try
            {
                var userId = _client.Auth.CurrentUser?.Id;
                if (string.IsNullOrEmpty(userId)) return new List<Invoice>();

                var response = await _client.From<Invoice>()
                    .Filter("user_id", Postgrest.Constants.Operator.Equals, userId)
                    .Get();
                var invoices = response.Models;

                foreach (var invoice in invoices)
                {
                    var oldStatus = invoice.Status;
                    invoice.UpdateStatus();
                    if (oldStatus != invoice.Status)
                    {
                        await _client.From<Invoice>().Update(invoice);
                    }
                }

                return invoices.OrderBy(i => i.DueDate).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching invoices: {ex.Message}");
                return new List<Invoice>();
            }
        }

        public async Task<List<Invoice>> GetInvoicesByStatusAsync(InvoiceStatus? status)
        {
            var all = await GetAllInvoicesAsync();
            if (status is null) return all;
            return all.Where(i => i.Status == status).ToList();
        }

        public async Task<List<Invoice>> SearchInvoicesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetAllInvoicesAsync();

            var all = await GetAllInvoicesAsync();
            var lowerQuery = query.ToLower();

            return all.Where(i =>
                i.Name.ToLower().Contains(lowerQuery) ||
                i.Amount.ToString().Contains(lowerQuery)
            ).ToList();
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(string id)
        {
            try
            {
                var userId = _client.Auth.CurrentUser?.Id;
                if (string.IsNullOrEmpty(userId)) return null;

                var response = await _client.From<Invoice>()
                    .Filter("id", Postgrest.Constants.Operator.Equals, id)
                    .Filter("user_id", Postgrest.Constants.Operator.Equals, userId)
                    .Get();
                return response.Models.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Invoice>> GetUpcomingInvoicesAsync()
        {
            var all = await GetAllInvoicesAsync();
            return all
                .Where(i => i.Status == InvoiceStatus.Pending
                         && i.DaysUntilDue >= 0
                         && i.DaysUntilDue <= 7)
                .OrderBy(i => i.DueDate)
                .ToList();
        }

        public async Task<List<Invoice>> GetOverdueInvoicesAsync()
        {
            var all = await GetAllInvoicesAsync();
            return all.Where(i => i.Status == InvoiceStatus.Overdue).ToList();
        }

        //WRITE

        public async Task<bool> AddInvoiceAsync(Invoice invoice)
        {
            if (string.IsNullOrWhiteSpace(invoice.Name) || invoice.Amount <= 0)
                return false;

            if (string.IsNullOrEmpty(invoice.Id))
                invoice.Id = Guid.NewGuid().ToString();

            if (invoice.CreatedAt == default)
                invoice.CreatedAt = DateTime.UtcNow;

            invoice.UserId = _client.Auth.CurrentUser?.Id ?? string.Empty;

            invoice.UpdateStatus();

            try
            {
                await _client.From<Invoice>().Insert(invoice);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Supabase Error: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateInvoiceAsync(Invoice invoice)
        {
            if (string.IsNullOrWhiteSpace(invoice.Name)) return false;
            invoice.UpdateStatus();
            try
            {
                await _client.From<Invoice>().Update(invoice);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAllInvoicesAsync()
        {
            try
            {
                var userId = _client.Auth.CurrentUser?.Id;
                if (string.IsNullOrEmpty(userId)) return false;

                await _client.From<Invoice>()
                    .Filter("user_id", Postgrest.Constants.Operator.Equals, userId)
                    .Delete();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting all invoices: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteInvoiceAsync(string id)
        {
            try
            {
                var userId = _client.Auth.CurrentUser?.Id;
                if (string.IsNullOrEmpty(userId)) return false;

                await _client.From<Invoice>()
                    .Filter("id", Postgrest.Constants.Operator.Equals, id)
                    .Filter("user_id", Postgrest.Constants.Operator.Equals, userId)
                    .Delete();
                return true;
            }
            catch
            {
                return false;
            }
        }

        //PAYMENT

        public async Task<bool> MarkAsPaidAsync(string invoiceId, PaymentMethod method)
        {
            var invoice = await GetInvoiceByIdAsync(invoiceId);
            if (invoice is null) return false;

            invoice.MarkAsPaid(method);
            await UpdateInvoiceAsync(invoice);

            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString(),
                InvoiceId = invoiceId,
                Amount = invoice.Amount,
                PaidAt = DateTime.Now,
                Method = method,
                IsPartial = false
            };

            try
            {
                await _client.From<Payment>().Insert(payment);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //STATISTICS

        public async Task<decimal> GetTotalAmountAsync()
        {
            var all = await GetAllInvoicesAsync();
            return all.Sum(i => i.Amount);
        }

        public async Task<decimal> GetTotalPaidAsync()
        {
            var all = await GetAllInvoicesAsync();
            return all.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.Amount);
        }

        public async Task<decimal> GetTotalToPayAsync()
        {
            var all = await GetAllInvoicesAsync();
            return all.Where(i => i.Status != InvoiceStatus.Paid).Sum(i => i.Amount);
        }

        public async Task<Dictionary<CategoryType, decimal>> GetExpensesByCategoryAsync()
        {
            var all = await GetAllInvoicesAsync();
            return all
                .GroupBy(i => i.Category)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Amount));
        }

        public async Task<List<(DateTime Month, decimal Amount)>> GetMonthlyExpensesAsync()
        {
            var all = await GetAllInvoicesAsync();
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            return all
                .Where(i => i.CreatedAt >= sixMonthsAgo)
                .GroupBy(i => new DateTime(i.CreatedAt.Year, i.CreatedAt.Month, 1))
                .Select(g => (Month: g.Key, Amount: g.Sum(i => i.Amount)))
                .OrderBy(x => x.Month)
                .ToList();
        }
    }
}