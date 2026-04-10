using BillWise.Models.Entities;
using BillWise.Resources.Strings;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using System.Text.Json;

namespace BillWise.Models.Services
{
    public class LocalNotificationScheduler
    {
        private readonly InvoiceService _invoiceService;
        private const string ReadIdsKey = "read_notification_ids";

        public LocalNotificationScheduler(InvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        public async Task ScheduleAsync()
        {
            var granted = await LocalNotificationCenter.Current.RequestNotificationPermission();
            if (!granted) return;

            var invoices = await _invoiceService.GetAllInvoicesAsync();
            var reminderDays = Preferences.Default.Get("reminder_days", 2);
            var readIds = GetReadIds();
            var L = LocalizationResourceManager.Instance;

            // Cancel all previously scheduled BillWise notifications then reschedule fresh
             LocalNotificationCenter.Current.CancelAll();

            // Stagger immediate notifications slightly so they don't overlap
            int delaySeconds = 5;

            foreach (var invoice in invoices)
            {
                if (invoice.Status == InvoiceStatus.Overdue)
                {
                    var id = $"overdue_{invoice.Id}";
                    if (readIds.Contains(id)) continue;

                    await LocalNotificationCenter.Current.Show(new NotificationRequest
                    {
                        NotificationId = ToIntId(id),
                        Title = L["NotifOverdueTitle"],
                        Description = string.Format(L["NotifOverdueMessage"], invoice.Name),
                        Schedule = new NotificationRequestSchedule
                        {
                            NotifyTime = DateTime.Now.AddSeconds(delaySeconds)
                        }
                    });
                    delaySeconds += 3;
                }
                else if (invoice.Status == InvoiceStatus.Pending)
                {
                    var id = $"duesoon_{invoice.Id}";
                    if (readIds.Contains(id)) continue;

                    if (invoice.DaysUntilDue <= reminderDays && invoice.DaysUntilDue >= 0)
                    {
                        // Already within the reminder window — fire soon
                        await LocalNotificationCenter.Current.Show(new NotificationRequest
                        {
                            NotificationId = ToIntId(id),
                            Title = L["NotifDueSoonTitle"],
                            Description = string.Format(L["NotifDueSoonMessage"], invoice.Name, invoice.DaysUntilDue),
                            Schedule = new NotificationRequestSchedule
                            {
                                NotifyTime = DateTime.Now.AddSeconds(delaySeconds)
                            }
                        });
                        delaySeconds += 3;
                    }
                    else if (invoice.DaysUntilDue > reminderDays)
                    {
                        // Schedule for the future (reminderDays before due date)
                        var notifyTime = invoice.DueDate.Date.AddDays(-reminderDays);
                        await LocalNotificationCenter.Current.Show(new NotificationRequest
                        {
                            NotificationId = ToIntId(id),
                            Title = L["NotifDueSoonTitle"],
                            Description = string.Format(L["NotifDueSoonMessage"], invoice.Name, reminderDays),
                            Schedule = new NotificationRequestSchedule
                            {
                                NotifyTime = notifyTime
                            }
                        });
                    }
                }
            }
        }

        // Convert a string ID to a stable positive int for the OS notification ID
        private static int ToIntId(string id)
            => Math.Abs(id.GetHashCode()) % 100_000;

        private static HashSet<string> GetReadIds()
        {
            var json = Preferences.Default.Get(ReadIdsKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return new HashSet<string>();
            return JsonSerializer.Deserialize<HashSet<string>>(json) ?? new HashSet<string>();
        }
    }
}
