namespace BillWise.Models.Services
{
    /// <summary>
    /// Service responsible for handling currency conversion and formatting.
    /// Assumes GBP (£) as the base currency.
    /// </summary>
    public static class CurrencyService
    {
        // Fixed exchange rates from GBP (base) — 1£ = 1.35$, 1£ = 1.15€
        private static readonly Dictionary<string, decimal> _fromGbp = new()
        {
            ["£"] = 1m,
            ["$"] = 1.35m,
            ["€"] = 1.15m
        };

        /// <summary>
        /// Gets the currently selected currency symbol from the user's preferences.
        /// Defaults to '£' if none is set.
        /// </summary>
        public static string Symbol => Preferences.Default.Get("currency", "£");

        /// <summary>
        /// Converts a given GBP amount to the currently selected currency based on fixed rates.
        /// </summary>
        /// <param name="gbpAmount">The monetary amount in GBP.</param>
        /// <returns>The converted decimal amount in the selected currency.</returns>
        public static decimal Convert(decimal gbpAmount)
        {
            var symbol = Symbol;
            return _fromGbp.TryGetValue(symbol, out var rate) ? gbpAmount * rate : gbpAmount;
        }

        /// <summary>
        /// Formats a given GBP amount into a localized string with the active currency symbol.
        /// E.g., placing the symbol before the value (£1234 or $1234).
        /// </summary>
        /// <param name="gbpAmount">The monetary amount in GBP.</param>
        /// <returns>A formatted currency string.</returns>
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
