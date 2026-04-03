using Postgrest.Attributes;
using Postgrest.Models;
using Newtonsoft.Json;

namespace BillWise.Models.Entities
{
    [Table("categories")]
    public class Category : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("type")]
        public CategoryType Type { get; set; }

        [Column("icon_name")]
        public string IconName { get; set; } = string.Empty;

        [Column("color")]
        public string Color { get; set; } = "#185FA5";

        // Static method to get default categories
        public static List<Category> GetDefaultCategories()
        {
            return new List<Category>
            {
                new Category { Name = "Electricity", Type = CategoryType.Electricity, IconName = "⚡", Color = "#F59E0B" },
                new Category { Name = "Water",       Type = CategoryType.Water,       IconName = "💧", Color = "#3B82F6" },
                new Category { Name = "Internet",    Type = CategoryType.Internet,    IconName = "🌐", Color = "#185FA5" },
                new Category { Name = "Rent",        Type = CategoryType.Rent,        IconName = "🏠", Color = "#EF4444" },
                new Category { Name = "Subscription",Type = CategoryType.Subscription,IconName = "📺", Color = "#8B5CF6" },
                new Category { Name = "Other",       Type = CategoryType.Other,       IconName = "📄", Color = "#6B7280" },
            };
        }
    }
}