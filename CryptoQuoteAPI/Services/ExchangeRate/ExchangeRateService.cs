using CryptoQuoteAPI.Configurations;
using CryptoQuoteAPI.Exceptions;
using CryptoQuoteAPI.Models.ExchangeRate;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CryptoQuoteAPI.Services.ExchangeRate
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly ExchangeRatesConfig _config;
        private readonly ILogger<ExchangeRateService> _logger;

        public ExchangeRateService(HttpClient httpClient, IOptions<ExchangeRatesConfig> config, ILogger<ExchangeRateService> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<List<ExchangeRateResult>> GetExchangeRatesAsync(string baseCurrency, IEnumerable<string> targetCurrencies, CancellationToken cancellationToken = default)
        {
            var symbols = string.Join(",", targetCurrencies);
            var requestUrl = $"latest?access_key={_config.ApiKey}&base={baseCurrency}&symbols={symbols}";

            HttpResponseMessage response;

            try
            {
                _logger.LogInformation("Sending request to ExchangeRatesAPI: {RequestUrl}", requestUrl);
                response = await _httpClient.GetAsync(requestUrl, cancellationToken);
                var res = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request error while fetching exchange rates.");
                throw new ExchangeRateApiException(0, "Unable to reach the exchange rates service.");
            }
            catch (TaskCanceledException tcEx) when (tcEx.CancellationToken == cancellationToken)
            {
                _logger.LogWarning("Exchange rates request was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching exchange rates.");
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("ExchangeRatesAPI returned an error. Status Code: {StatusCode}, Content: {Content}",
                              response.StatusCode, errorContent);
                throw new ExchangeRateApiException((int)response.StatusCode, $"ExchangeRatesAPI error: {response.ReasonPhrase}");
            }

            ExchangeRatesResponse? data;
            try
            {
                var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                data = await JsonSerializer.DeserializeAsync<ExchangeRatesResponse>(contentStream, options, cancellationToken);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error while parsing exchange rates.");
                throw new ExchangeRateApiException((int)response.StatusCode, "Invalid response format from exchange rates service.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deserializing exchange rates response.");
                throw;
            }

            if (data == null || data.Rates == null)
            {
                _logger.LogError("ExchangeRatesAPI returned null or incomplete data.");
                throw new ExchangeRateApiException((int)response.StatusCode, "Incomplete data received from exchange rates service.");
            }

            var results = new List<ExchangeRateResult>();

            foreach (var currency in targetCurrencies)
            {
                if (data.Rates.TryGetValue(currency.ToUpper(), out var rate))
                {
                    results.Add(new ExchangeRateResult
                    {
                        Currency = currency.ToUpper(),
                        Rate = rate
                    });
                }
                else
                {
                    _logger.LogWarning("Exchange rate for currency '{Currency}' not found.", currency);
                    results.Add(new ExchangeRateResult
                    {
                        Currency = currency.ToUpper(),
                        Rate = null,
                        Error = $"Exchange rate for currency '{currency}' is not available."
                    });
                }
            }

            return results;
        }
    }
}
