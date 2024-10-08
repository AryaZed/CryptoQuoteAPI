namespace CryptoQuoteAPI.Models.Base
{
    public class PriceResult
    {
        public string Currency { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string? Error { get; set; }
    }
}
