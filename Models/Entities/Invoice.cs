using BillWise.Models.Services;
using BillWise.Resources.Strings;
using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace BillWise.Models.Entities
{
    public enum InvoiceStatus { Pending, Paid, Overdue }
    public enum PaymentMethod { BankTransfer, PayPal, GooglePay, Cash, CardPayment, DirectDebit }
    public enum CategoryType { Electricity, Water, Internet, Rent, Subscription, Other }

    [Table("invoices")]
    public class Invoice : BaseModel
    {
        [PrimaryKey("id", true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;
        
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

        // --- UI computed properties (used by XAML bindings) ---
        [JsonIgnore] public string FormattedAmount => CurrencyService.Format(Amount);

        [JsonIgnore]
        public string StatusText => Status switch
        {
            InvoiceStatus.Paid    => LocalizationResourceManager.Instance["Paid"],
            InvoiceStatus.Overdue => LocalizationResourceManager.Instance["Overdue"],
            _                     => LocalizationResourceManager.Instance["Pending"]
        };

        [JsonIgnore]
        public string StatusBackgroundColor => Status switch
        {
            InvoiceStatus.Paid    => "#D1FAE5",
            InvoiceStatus.Overdue => "#FEE2E2",
            _                     => "#FFF9C4"
        };

        [JsonIgnore]
        public string StatusTextColor => Status switch
        {
            InvoiceStatus.Paid    => "#065F46",
            InvoiceStatus.Overdue => "#B91C1C",
            _                     => "#92400E"
        };

        [JsonIgnore]
        public string CategoryIcon => Category switch
        {
            CategoryType.Electricity  => "⚡",
            CategoryType.Water        => "💧",
            CategoryType.Internet     => "🌐",
            CategoryType.Rent         => "🏠",
            CategoryType.Subscription => "📺",
            _                         => "📄"
        };

        [JsonIgnore]
        public string CategoryIconBackgroundColor => Category switch
        {
            CategoryType.Electricity  => "#FEF3C7",
            CategoryType.Water        => "#DBEAFE",
            CategoryType.Internet     => "#EDE9FE",
            CategoryType.Rent         => "#FEE2E2",
            CategoryType.Subscription => "#F3E8FF",
            _                         => "#F3F4F6"
        };

        [JsonIgnore]
        public string CategoryText => Category switch
        {
            CategoryType.Electricity  => LocalizationResourceManager.Instance["Electricity"],
            CategoryType.Water        => LocalizationResourceManager.Instance["Water"],
            CategoryType.Internet     => LocalizationResourceManager.Instance["Internet"],
            CategoryType.Rent         => LocalizationResourceManager.Instance["Rent"],
            CategoryType.Subscription => LocalizationResourceManager.Instance["Subscription"],
            _                         => LocalizationResourceManager.Instance["Other"]
        };

        [JsonIgnore]
        public string PaymentMethodText => PaymentMethod switch
        {
            Entities.PaymentMethod.PayPal       => LocalizationResourceManager.Instance["PaymentPayPal"],
            Entities.PaymentMethod.GooglePay    => LocalizationResourceManager.Instance["PaymentGooglePay"],
            _                                   => LocalizationResourceManager.Instance["PaymentBankTransfer"]
        };

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