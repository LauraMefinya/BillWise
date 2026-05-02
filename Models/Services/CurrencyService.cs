namespace BillWise.Models.Services
{
    public static class CurrencyService
    {
        // Fixed rates from GBP (base) — 1£ = 1.35$, 1£ = 1.15€
        private static readonly Dictionary<string, decimal> _fromGbp = new()
        {
            ["£"] = 1m,
            ["$"] = 1.35m,
            ["€"] = 1.15m
        };

        public static string Symbol => Preferences.Default.Get("currency", "£");

        public static decimal Convert(decimal gbpAmount)
        {
            var symbol = Symbol;
            return _fromGbp.TryGetValue(symbol, out var rate) ? gbpAmount * rate : gbpAmount;
        }

        public static string Format(decimal gbpAmount)
        {
            var symbol = Symbol;
            var converted = _fromGbp.TryGetValue(symbol, out var rate) ? gbpAmount * rate : gbpAmount;
            return converted % 1 == 0
                ? $"{symbol}{converted:N0}"
                : $"{symbol}{converted:N2}";
        }
    }
}
