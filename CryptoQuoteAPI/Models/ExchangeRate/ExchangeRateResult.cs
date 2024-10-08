namespace CryptoQuoteAPI.Models.ExchangeRate
{
    public class ExchangeRateResult
    {
        public string Currency { get; set; } = string.Empty;
        public decimal? Rate { get; set; }
        public string? Error { get; set; }
    }
}
