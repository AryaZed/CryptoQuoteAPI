using CryptoQuoteAPI.Hubs;
using CryptoQuoteAPI.Services.CoinMarketCap;
using Microsoft.AspNetCore.SignalR;

namespace CryptoQuoteAPI.Services.BackgroundServices
{
    /// <summary>
    /// Background service to periodically fetch and broadcast cryptocurrency prices.
    /// </summary>
    public class BackgroundCryptoUpdateService : BackgroundService
    {
        private readonly ICryptoService _cryptoService;
        private readonly IHubContext<CryptoHub> _hubContext;
        private readonly ILogger<BackgroundCryptoUpdateService> _logger;
        private readonly string _cryptoCode = "BTC"; // You can make this configurable
        private readonly List<string> _currencies = new List<string> { "USD", "EUR", "BRL", "GBP", "AUD" };
        private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(5);

        public BackgroundCryptoUpdateService(
            ICryptoService cryptoService,
            IHubContext<CryptoHub> hubContext,
            ILogger<BackgroundCryptoUpdateService> logger)
        {
            _cryptoService = cryptoService;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundCryptoUpdateService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Fetching cryptocurrency prices for {CryptoCode}.", _cryptoCode);
                    var prices = await _cryptoService.GetCryptoPricesAsync(_cryptoCode, _currencies, stoppingToken);

                    foreach (var price in prices)
                    {
                        if (price.Price.HasValue)
                        {
                            await _hubContext.Clients.All.SendAsync(
                                "ReceiveCryptoUpdate",
                                _cryptoCode,
                                price.Currency,
                                price.Price.Value,
                                stoppingToken);
                        }
                    }

                    _logger.LogInformation("Successfully pushed cryptocurrency prices to clients.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while fetching or pushing cryptocurrency prices.");
                }

                await Task.Delay(_updateInterval, stoppingToken);
            }

            _logger.LogInformation("BackgroundCryptoUpdateService is stopping.");
        }
    }
}
