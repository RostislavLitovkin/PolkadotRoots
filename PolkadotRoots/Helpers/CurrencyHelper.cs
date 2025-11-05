using System.Globalization;
using System.Text.Json;

namespace PolkadotRoots.Helpers
{
    internal class CurrencyHelper
    {
        // Optional aliases for common-but-nonstandard country codes.
        private static readonly Dictionary<string, string> CountryAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            ["UK"] = "GB" // accept "UK" as Great Britain
        };

        // Reuse a single HttpClient instance.
        private static readonly HttpClient Http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        public static async Task<(string CurrencySymbol, string IsoCurrencySymbol, decimal RateToUSD)> GetCurrencySymbolAndRateToUsdAsync(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
                throw new ArgumentException("Country code must be a non-empty ISO 3166-1 alpha-2 code.", nameof(countryCode));

            countryCode = countryCode.Trim();

            // Normalize nonstandard inputs like "UK".
            if (CountryAliases.TryGetValue(countryCode, out var mapped))
                countryCode = mapped;

            // Validate format.
            if (countryCode.Length != 2)
                throw new ArgumentException("Country code must be 2 letters (ISO 3166-1 alpha-2).", nameof(countryCode));

            // Get region/currency info.
            RegionInfo region;
            try
            {
                region = new RegionInfo(countryCode.ToUpperInvariant());
            }
            catch (ArgumentException)
            {
                throw new ArgumentException($"Unrecognized country code: '{countryCode}'.", nameof(countryCode));
            }

            string symbol = region.CurrencySymbol;       // e.g., "Kč", "$", "€"
            string isoCurrency = region.ISOCurrencySymbol; // e.g., "CZK", "USD", "EUR"

            // If already USD, the rate is 1 by definition.
            if (string.Equals(isoCurrency, "USD", StringComparison.OrdinalIgnoreCase))
                return (symbol, isoCurrency, 1m);

            // Fetch USD-based rates (1 USD -> X [currency]); we'll invert to get [currency] -> USD.
            // Free, no-auth endpoint. Consider swapping for your preferred/paid provider in production.
            var url = "https://open.er-api.com/v6/latest/USD";
            using var resp = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            if (!doc.RootElement.TryGetProperty("result", out var resultEl) ||
                !string.Equals(resultEl.GetString(), "success", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Exchange-rate service did not return success.");
            }

            if (!doc.RootElement.TryGetProperty("rates", out var ratesEl) ||
                ratesEl.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("Exchange-rate payload missing 'rates'.");
            }

            if (!ratesEl.TryGetProperty(isoCurrency.ToUpperInvariant(), out var usdToLocalEl))
            {
                throw new KeyNotFoundException($"USD->'{isoCurrency}' rate not found from provider.");
            }

            // Provider gives 1 USD = R localCurrency. We want 1 localCurrency = (1/R) USD.
            decimal usdToLocal = usdToLocalEl.GetDecimal();
            if (usdToLocal <= 0m)
                throw new InvalidOperationException($"Received non-positive rate for {isoCurrency}.");

            decimal localToUsd = 1m / usdToLocal;

            return (symbol, isoCurrency, localToUsd);
        }
    }
}
