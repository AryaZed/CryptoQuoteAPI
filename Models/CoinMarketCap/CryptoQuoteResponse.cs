namespace CryptoQuoteAPI.Models.CoinMarketCap
{
    public class CryptoQuoteResponse
    {
        /// <summary>
        /// The target currency code (e.g., USD, EUR).
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// The price of the cryptocurrency in the target currency.
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Error message if the price or exchange rate is unavailable.
        /// </summary>
        public string? Error { get; set; }
    }
}
