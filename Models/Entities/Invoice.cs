using Postgrest.Attributes;
using Postgrest.Models;
using Newtonsoft.Json;

namespace BillWise.Models.Entities
{
    public enum InvoiceStatus { Pending, Paid, Overdue }
    public enum PaymentMethod { Cash, MobileMoney, BankTransfer }
    public enum CategoryType { Electricity, Water, Internet, Rent, Subscription, Other }

    [Table("invoices")]
    public class Invoice : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Column("name")]
        public string Name { get; set; } = string.Empty;
        
        [Column("amount")]
        public decimal Amount { get; set; }
        
        [Column("due_date")]
        public DateTime DueDate { get; set; }
        
        [Column("status")]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
        
        [Column("notes")]
        public string? Notes { get; set; }
        
        [Column("attachment_path")]
        public string? AttachmentPath { get; set; }
        
        [Column("location")]
        public string? Location { get; set; }
        
        [Column("category")]
        public CategoryType Category { get; set; } = CategoryType.Other;
        
        [Column("payment_method")]
        public PaymentMethod? PaymentMethod { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Column("paid_at")]
        public DateTime? PaidAt { get; set; }

        [JsonIgnore] public bool IsOverdue => Status != InvoiceStatus.Paid && DueDate < DateTime.Today;
        [JsonIgnore] public int DaysUntilDue => (DueDate - DateTime.Today).Days;
        [JsonIgnore] public string StatusLabel => Status switch
        {
            InvoiceStatus.Paid => "Paid",
            InvoiceStatus.Overdue => "Overdue",
            _ => "Pending"
        };
        [JsonIgnore] public string AmountFormatted => $"{Amount:N0} FCFA";

        // Display-only UI properties
        [JsonIgnore] public string CategoryIcon { get; set; } = "📄";
        [JsonIgnore] public string StatusBadgeColor { get; set; } = "#FFF9C4";
        [JsonIgnore] public string StatusTextColor { get; set; } = "#F57F17";

        public void MarkAsPaid(PaymentMethod method)
        {
            Status = InvoiceStatus.Paid;
            PaidAt = DateTime.Now;
            PaymentMethod = method;
        }

        public void UpdateStatus()
        {
            if (Status != InvoiceStatus.Paid)
                Status = DueDate < DateTime.Today ? InvoiceStatus.Overdue : InvoiceStatus.Pending;
        }
    }
}