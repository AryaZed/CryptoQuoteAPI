namespace CryptoQuoteAPI.Models.CoinMarketCap
{
    public class CoinMarketCapResponse
    {
        public Dictionary<string, CryptoData> Data { get; set; } = new();
    }

    public class CryptoData
    {
        public Dictionary<string, QuoteData> Quote { get; set; } = new();
    }

    public class QuoteData
    {
        public decimal Price { get; set; }
    }
}
