using Postgrest.Attributes;
using Postgrest.Models;

namespace BillWise.Models.Entities
{
    /// <summary>
    /// Represents a user profile in the database, mapped to the 'profiles' table.
    /// This entity stores the user's basic information such as full name and email.
    /// </summary>
    [Table("profiles")]
    public class UserProfile : BaseModel
    {
        /// <summary>
        /// Gets or sets the unique identifier of the user profile, which matches the Supabase auth.users ID.
        /// </summary>
        [PrimaryKey("id", true)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [Column("email")]
        public string Email { get; set; } = string.Empty;
    }
}
