using CryptoQuoteAPI.Models.Base;

namespace CryptoQuoteAPI.Services.CoinMarketCap
{
    public interface ICryptoService
    {
        Task<List<PriceResult>> GetCryptoPricesAsync(string cryptoCode, IEnumerable<string> currencies, CancellationToken cancellationToken = default);
    }
}
