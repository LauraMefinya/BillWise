using BillWise.Models.Entities;
using BillWise.Resources.Strings;
using System.Text.Json;

namespace BillWise.Models.Services
{
    public class NotificationService
    {
        private readonly InvoiceService _invoiceService;
        private const string ReadIdsKey = "read_notification_ids";

        public NotificationService(InvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        public async Task<List<AppNotification>> GetNotificationsAsync()
        {
            var invoices = await _invoiceService.GetAllInvoicesAsync();
            var reminderDays = Preferences.Default.Get("reminder_days", 2);
            var readIds = GetReadIds();
            var notifications = new List<AppNotification>();
            var L = LocalizationResourceManager.Instance;

            foreach (var invoice in invoices.OrderByDescending(i => i.DueDate))
            {
                if (invoice.Status == InvoiceStatus.Overdue)
                {
                    var id = $"overdue_{invoice.Id}";
                    notifications.Add(new AppNotification
                    {
                        Id = id,
                        Title = L["NotifOverdueTitle"],
                        Message = string.Format(L["NotifOverdueMessage"], invoice.Name),
                        Type = "danger",
                        SentAt = invoice.DueDate,
                        IsRead = readIds.Contains(id),
                        InvoiceId = invoice.Id
                    });
                }
                else if (invoice.Status == InvoiceStatus.Pending
                         && invoice.DaysUntilDue >= 0
                         && invoice.DaysUntilDue <= reminderDays)
                {
                    var id = $"duesoon_{invoice.Id}";
                    notifications.Add(new AppNotification
                    {
                        Id = id,
                        Title = L["NotifDueSoonTitle"],
                        Message = string.Format(L["NotifDueSoonMessage"], invoice.Name, invoice.DaysUntilDue),
                        Type = "warning",
                        SentAt = DateTime.Now,
                        IsRead = readIds.Contains(id),
                        InvoiceId = invoice.Id
                    });
                }
            }

            return notifications.OrderByDescending(n => n.SentAt).ToList();
        }

        public void MarkAsRead(string id)
        {
            var ids = GetReadIds();
            ids.Add(id);
            SaveReadIds(ids);
        }

        public void MarkAllAsRead(IEnumerable<string> ids)
        {
            var existing = GetReadIds();
            foreach (var id in ids)
                existing.Add(id);
            SaveReadIds(existing);
        }

        private HashSet<string> GetReadIds()
        {
            var json = Preferences.Default.Get(ReadIdsKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return new HashSet<string>();
            return JsonSerializer.Deserialize<HashSet<string>>(json) ?? new HashSet<string>();
        }

        private void SaveReadIds(HashSet<string> ids)
        {
            Preferences.Default.Set(ReadIdsKey, JsonSerializer.Serialize(ids));
        }
    }
}
