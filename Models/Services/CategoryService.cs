using System.Text.Json;
using System.Text.Json.Serialization;

namespace BillWise.Models.Services
{
    public class CustomCategory
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = "📄";

        public CustomCategory() { }
        public CustomCategory(string name, string icon) { Name = name; Icon = icon; }
    }

    public static class CategoryService
    {
        private const string KeyV2 = "billwise_custom_cats_v2";
        private const string KeyLegacy = "billwise_custom_categories";

        public static List<CustomCategory> GetCustomCategories()
        {
            var json = Preferences.Default.Get(KeyV2, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                try { return JsonSerializer.Deserialize<List<CustomCategory>>(json) ?? new(); }
                catch { }
            }

            // Migrate legacy string-only storage
            var legacy = Preferences.Default.Get(KeyLegacy, "[]");
            try
            {
                var names = JsonSerializer.Deserialize<List<string>>(legacy) ?? new();
                var migrated = names.Select(n => new CustomCategory(n, "📄")).ToList();
                if (migrated.Count > 0)
                    Preferences.Default.Set(KeyV2, JsonSerializer.Serialize(migrated));
                return migrated;
            }
            catch { return new(); }
        }

        public static string GetIconForCategory(string name)
        {
            return GetCustomCategories()
                .FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                ?.Icon ?? "📄";
        }

        public static void SaveCustomCategory(string name, string icon = "📄")
        {
            var list = GetCustomCategories();
            if (list.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))) return;
            list.Add(new CustomCategory(name, icon));
            Preferences.Default.Set(KeyV2, JsonSerializer.Serialize(list));
        }

        public static void RemoveCustomCategory(string name)
        {
            var list = GetCustomCategories();
            list.RemoveAll(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            Preferences.Default.Set(KeyV2, JsonSerializer.Serialize(list));
        }
    }
}
