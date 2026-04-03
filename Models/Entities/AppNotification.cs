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

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}