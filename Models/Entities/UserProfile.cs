using Postgrest.Attributes;
using Postgrest.Models;

namespace BillWise.Models.Entities
{
    [Table("profiles")]
    public class UserProfile : BaseModel
    {
        [PrimaryKey("id", true)]
        public string Id { get; set; } = string.Empty;

        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;
    }
}
