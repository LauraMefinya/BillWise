using System.Text.Json;

namespace BillWise.Models.Services
{
    public static class CategoryService
    {
        private const string Key = "billwise_custom_categories";

        public static List<string> GetCustomCategories()
        {
            var json = Preferences.Default.Get(Key, "[]");
            try { return JsonSerializer.Deserialize<List<string>>(json) ?? new(); }
            catch { return new(); }
        }

        public static void SaveCustomCategory(string name)
        {
            var list = GetCustomCategories();
            if (list.Any(c => c.Equals(name, StringComparison.OrdinalIgnoreCase))) return;
            list.Add(name);
            Preferences.Default.Set(Key, JsonSerializer.Serialize(list));
        }

        public static void RemoveCustomCategory(string name)
        {
            var list = GetCustomCategories();
            list.RemoveAll(c => c.Equals(name, StringComparison.OrdinalIgnoreCase));
            Preferences.Default.Set(Key, JsonSerializer.Serialize(list));
        }
    }
}
