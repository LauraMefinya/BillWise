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
        public PaymentMethod Method { get; set; } = PaymentMethod.Cash;

        [Column("reference")]
        public string? Reference { get; set; }

        [Column("is_partial")]
        public bool IsPartial { get; set; } = false;

        [JsonIgnore]
        public string AmountFormatted => $"{Amount:N0} FCFA";

        [JsonIgnore]
        public string MethodLabel => Method switch
        {
            PaymentMethod.Cash => "Cash",
            PaymentMethod.MobileMoney => "Mobile Money",
            PaymentMethod.BankTransfer => "Bank Transfer",
            _ => "Cash"
        };
    }
}