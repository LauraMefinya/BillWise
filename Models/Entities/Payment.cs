using BillWise.Models.Services;
using Postgrest.Attributes;
using Postgrest.Models;
using Newtonsoft.Json;

namespace BillWise.Models.Entities
{
    [Table("payments")]
    public class Payment : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Column("invoice_id")]
        public string InvoiceId { get; set; } = string.Empty;

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

        [JsonIgnore]
        public string AmountFormatted => CurrencyService.Format(Amount);

        [JsonIgnore]
        public string MethodLabel => Method switch
        {
            PaymentMethod.PayPal    => "PayPal",
            PaymentMethod.GooglePay => "Google Pay",
            _                       => "Bank Transfer"
        };
    }
}