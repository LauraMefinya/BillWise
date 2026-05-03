using Postgrest.Attributes;
using Postgrest.Models;
using Newtonsoft.Json;

namespace BillWise.Models.Entities
{
    /// <summary>
    /// Represents the user's application settings stored in the database.
    /// Maps to the 'settings' table in Supabase.
    /// </summary>
    [Table("settings")]
    public class AppSettings : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = "default";

        [Column("currency")]
        public string Currency { get; set; } = "FCFA";

        [Column("language")]
        public string Language { get; set; } = "English";

        [Column("notifications_enabled")]
        public bool NotificationsEnabled { get; set; } = true;

        [Column("haptic_feedback_enabled")]
        public bool HapticFeedbackEnabled { get; set; } = true;

        [Column("shake_to_add_enabled")]
        public bool ShakeToAddEnabled { get; set; } = true;

        [Column("reminder_days_before")]
        public int ReminderDaysBefore { get; set; } = 3;

        [Column("biometric_enabled")]
        public bool BiometricEnabled { get; set; } = false;

        [Column("user_name")]
        public string UserName { get; set; } = "User";

        [Column("user_email")]
        public string UserEmail { get; set; } = "user@example.com";
    }
}