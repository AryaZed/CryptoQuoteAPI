namespace CryptoQuoteAPI.Models.ExchangeRate
{
    public class ExchangeRatesResponse
    {
        public string Base { get; set; } = string.Empty;
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}
