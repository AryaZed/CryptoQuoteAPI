using CryptoQuoteAPI.Models.ExchangeRate;

namespace CryptoQuoteAPI.Services.ExchangeRate;

public interface IExchangeRateService
{
    Task<List<ExchangeRateResult>> GetExchangeRatesAsync(string baseCurrency, IEnumerable<string> targetCurrencies, CancellationToken cancellationToken = default);
}