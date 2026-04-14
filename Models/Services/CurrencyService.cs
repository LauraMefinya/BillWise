namespace BillWise.Models.Services
{
    public static class CurrencyService
    {
        public static string Symbol => Preferences.Default.Get("currency", "£");
        public static string Format(decimal amount) => $"{amount:N0} {Symbol}";
    }
}
