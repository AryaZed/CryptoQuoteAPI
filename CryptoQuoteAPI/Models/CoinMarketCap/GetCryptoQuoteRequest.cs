using System.ComponentModel.DataAnnotations;

namespace CryptoQuoteAPI.Models.CoinMarketCap
{
    public class GetCryptoQuoteRequest
    {
        /// <summary>
        /// The cryptocurrency code (e.g., BTC, ETH).
        /// </summary>
        [Required(ErrorMessage = "Cryptocurrency code is required.")]
        public string CryptoCode { get; set; } = string.Empty;

        /// <summary>
        /// Optional list of target currencies (e.g., USD, EUR).
        /// </summary>
        public IEnumerable<string>? Currencies { get; set; }
    }
}
