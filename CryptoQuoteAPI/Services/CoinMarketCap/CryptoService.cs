using CryptoQuoteAPI.Configurations;
using CryptoQuoteAPI.Exceptions;
using CryptoQuoteAPI.Models.Base;
using CryptoQuoteAPI.Models.CoinMarketCap;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Caching.Memory;
using CryptoQuoteAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CryptoQuoteAPI.Services.CoinMarketCap
{
    public class CryptoService : ICryptoService
    {
        private readonly HttpClient _httpClient;
        private readonly CoinMarketCapConfig _config;
        private readonly ILogger<CryptoService> _logger;
        private readonly IMemoryCache _cache;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(5);

        public CryptoService(HttpClient httpClient, IOptions<CoinMarketCapConfig> config, ILogger<CryptoService> logger, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;
            _cache = cache;

            if (!_httpClient.DefaultRequestHeaders.Contains("X-CMC_PRO_API_KEY"))
            {
                _httpClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", _config.ApiKey);
            }
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));            
        }

        private async Task<PriceResult> FetchPriceAsync(string cryptoCode, string currency, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                var requestUrl = $"v1/cryptocurrency/quotes/latest?symbol={cryptoCode}&convert={currency}";

                _logger.LogInformation("Sending request to CoinMarketCap API: {RequestUrl}", requestUrl);

                HttpResponseMessage response;

                try
                {
                    response = await _httpClient.GetAsync(requestUrl, cancellationToken);
                }
                catch (HttpRequestException httpEx)
                {
                    _logger.LogError(httpEx, "HTTP request error while fetching cryptocurrency prices for {Currency}.", currency);
                    throw new Exception("Unable to reach the cryptocurrency service.", httpEx);
                }
                catch (TaskCanceledException tcEx) when (tcEx.CancellationToken == cancellationToken)
                {
                    _logger.LogWarning("Request to CoinMarketCap API was canceled for {Currency}.", currency);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while fetching cryptocurrency prices for {Currency}.", currency);
                    throw;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("CoinMarketCap API error: {StatusCode}, Content: {Content} for {Currency}", response.StatusCode, errorContent, currency);
                    throw new Exception($"Failed to retrieve cryptocurrency prices for {currency}. API error: {response.ReasonPhrase}");
                }

                CoinMarketCapResponse? data;
                try
                {
                    var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    data = await JsonSerializer.DeserializeAsync<CoinMarketCapResponse>(contentStream, options, cancellationToken);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "JSON deserialization error while parsing CoinMarketCap API response for {Currency}.", currency);
                    throw new Exception("Invalid response format from cryptocurrency service.", jsonEx);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while deserializing CoinMarketCap API response for {Currency}.", currency);
                    throw;
                }

                if (data != null && data.Data != null && data.Data.TryGetValue(cryptoCode, out var cryptoData))
                {
                    if (cryptoData.Quote.TryGetValue(currency.ToUpper(), out var quoteData))
                    {
                        return new PriceResult
                        {
                            Currency = currency.ToUpper(),
                            Price = quoteData.Price
                        };
                    }
                    else
                    {
                        _logger.LogWarning("Currency '{Currency}' is not available in CoinMarketCap API response.", currency);
                        return new PriceResult
                        {
                            Currency = currency.ToUpper(),
                            Price = null,
                            Error = $"Currency '{currency}' is not available."
                        };
                    }
                }
                else
                {
                    throw new CryptocurrencyNotFoundException(cryptoCode);
                }
            }
            finally
            {
                _semaphore.Release();
            }          
        }

        public async Task<List<PriceResult>> GetCryptoPricesAsync(string cryptoCode, IEnumerable<string> currencies, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"CryptoPrices_{cryptoCode}_" + string.Join("_", currencies.Select(c => c.ToUpper()));

            if (_cache.TryGetValue(cacheKey, out List<PriceResult> cachedPrices))
            {
                _logger.LogInformation("Returning cached cryptocurrency prices for {CryptoCode}.", cryptoCode);
                return cachedPrices;
            }

            var fetchTasks = currencies.Select(currency => FetchPriceAsync(cryptoCode, currency, cancellationToken));

            List<PriceResult> results;

            try
            {
                results = (await Task.WhenAll(fetchTasks)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching cryptocurrency prices.");
                throw;
            }

            // Cache the aggregated results for 5 minutes
            _cache.Set(cacheKey, results, TimeSpan.FromMinutes(5));

            return results;
        }

    }
}
