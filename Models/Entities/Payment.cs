using BillWise.Models.Services;
using Postgrest.Attributes;
using Postgrest.Models;
using Newtonsoft.Json;

namespace BillWise.Models.Entities
{
    /// <summary>
    /// Represents a payment made towards an invoice.
    /// Maps to the 'payments' table in Supabase.
    /// </summary>
    [Table("payments")]
    public class Payment : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Column("invoice_id")]
        public string InvoiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the monetary amount of this specific payment.
        /// </summary>
        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("paid_at")]
        public DateTime PaidAt { get; set; } = DateTime.Now;

        [Column("method")]
        public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;

        [Column("reference")]
        public string? Reference { get; set; }

        [Column("is_partial")]
        public bool IsPartial { get; set; } = false;

        /// <summary>
        /// Gets the fully formatted currency string for this payment's amount.
        /// </summary>
        [JsonIgnore]
        public string AmountFormatted => CurrencyService.Format(Amount);

        [JsonIgnore]
        public string MethodLabel => Method switch
        {
            PaymentMethod.PayPal      => "PayPal",
            PaymentMethod.GooglePay   => "Google Pay",
            PaymentMethod.Cash        => "Cash",
            PaymentMethod.CardPayment => "Card Payment",
            PaymentMethod.DirectDebit => "Direct Debit",
            _                         => "Bank Transfer"
        };
    }
}