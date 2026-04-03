using Postgrest.Attributes;
using Postgrest.Models;
using Newtonsoft.Json;

namespace BillWise.Models.Entities
{
    [Table("notifications")]
    public class AppNotification : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Column("sent_at")]
        public DateTime SentAt { get; set; } = DateTime.Now;

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [JsonIgnore]
        public bool IsUnread => !IsRead;

        [Column("type")]
        public string Type { get; set; } = "info";

        [Column("invoice_id")]
        public string? InvoiceId { get; set; }

        // --- UI computed properties ---
        [JsonIgnore]
        public string TypeIcon => Type switch
        {
            "danger"  => "⚠️",
            "warning" => "📅",
            _         => "🔔"
        };

        [JsonIgnore]
        public string TypeIconBgColor => Type switch
        {
            "danger"  => "#FEE2E2",
            "warning" => "#DBEAFE",
            _         => "#F3F4F6"
        };

        [JsonIgnore]
        public string TypeCardBgColor => Type switch
        {
            "danger"  => "#FFF5F5",
            "warning" => "#EFF6FF",
            _         => "#FFFFFF"
        };

        [JsonIgnore]
        public string TypeBorderColor => Type switch
        {
            "danger"  => "#FECACA",
            "warning" => "#BFDBFE",
            _         => "#E5E7EB"
        };

        [JsonIgnore]
        public string TypeTitleColor => Type switch
        {
            "danger"  => "#B91C1C",
            "warning" => "#1D4ED8",
            _         => "#111827"
        };

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}