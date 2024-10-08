using CryptoQuoteAPI;
using CryptoQuoteAPI.Endpoints;
using CryptoQuoteAPI.Models.Base;
using CryptoQuoteAPI.Models.CoinMarketCap;
using CryptoQuoteAPI.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using CryptoQuoteAPI.Services.CoinMarketCap;
using CryptoQuoteAPI.Services.ExchangeRate;

namespace CryptoQuoteAPI.Tests
{
    public class CryptoEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<ICryptoService> _mockCryptoService;
        private readonly Mock<IExchangeRateService> _mockExchangeRateService;
        private readonly Mock<ICryptoSettingsService> _mockCryptoSettingsService;

        public CryptoEndpointTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _mockCryptoService = new Mock<ICryptoService>();
            _mockExchangeRateService = new Mock<IExchangeRateService>();
            _mockCryptoSettingsService = new Mock<ICryptoSettingsService>();
        }

        [Fact]
        public async Task GetCryptoQuote_ReturnsOk_WithValidData()
        {
            // Arrange
            var cryptoCode = "BTC";
            var currencies = new[] { "USD", "EUR" };

            // 2. Set Up Mock Behaviors
            _mockCryptoService.Setup(service => service.GetCryptoPricesAsync(
                cryptoCode,
                It.IsAny<List<string>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PriceResult>
                {
                    new PriceResult { Currency = "USD", Price = 50000 },
                    new PriceResult { Currency = "EUR", Price = 42000 }
                });

            // 3. Inject Mocks into Test Environment
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing service registrations if necessary
                    var descriptorCrypto = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ICryptoService));
                    if (descriptorCrypto != null)
                    {
                        services.Remove(descriptorCrypto);
                    }

                    var descriptorExchange = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IExchangeRateService));
                    if (descriptorExchange != null)
                    {
                        services.Remove(descriptorExchange);
                    }

                    var descriptorSettings = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ICryptoSettingsService));
                    if (descriptorSettings != null)
                    {
                        services.Remove(descriptorSettings);
                    }

                    // Register mocked services
                    services.AddSingleton<ICryptoService>(_mockCryptoService.Object);
                    services.AddSingleton<IExchangeRateService>(_mockExchangeRateService.Object);
                    services.AddSingleton<ICryptoSettingsService>(_mockCryptoSettingsService.Object);
                });
            }).CreateClient();

            var requestUrl = $"/crypto/{cryptoCode}?{string.Join("&", currencies.Select(c => $"currencies={c}"))}";

            // Act
            var response = await client.GetAsync(requestUrl);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<List<CryptoQuoteResponse>>(responseContent);
            results.Should().HaveCount(2);
            results.Should().Contain(r => r.Currency == "USD" && r.Price == 50000);
            results.Should().Contain(r => r.Currency == "EUR" && r.Price == 42000);
        }

        [Fact]
        public async Task GetCryptoQuote_ReturnsBadRequest_WithInvalidCryptoCode()
        {
            // Arrange
            var cryptoCode = "INVALID_CODE";
            var currencies = new[] { "USD", "EUR" };

            // 2. Set Up Mock Behaviors for Invalid CryptoCode
            _mockCryptoService.Setup(service => service.GetCryptoPricesAsync(
                cryptoCode,
                It.IsAny<List<string>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PriceResult>());

            // 3. Inject Mocks into Test Environment
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing service registrations if necessary
                    var descriptorCrypto = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ICryptoService));
                    if (descriptorCrypto != null)
                    {
                        services.Remove(descriptorCrypto);
                    }

                    var descriptorExchange = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IExchangeRateService));
                    if (descriptorExchange != null)
                    {
                        services.Remove(descriptorExchange);
                    }

                    var descriptorSettings = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ICryptoSettingsService));
                    if (descriptorSettings != null)
                    {
                        services.Remove(descriptorSettings);
                    }

                    // Register mocked services
                    services.AddSingleton<ICryptoService>(_mockCryptoService.Object);
                    services.AddSingleton<IExchangeRateService>(_mockExchangeRateService.Object);
                    services.AddSingleton<ICryptoSettingsService>(_mockCryptoSettingsService.Object);
                });
            }).CreateClient();

            var requestUrl = $"/crypto/{cryptoCode}?{string.Join("&", currencies.Select(c => $"currencies={c}"))}";

            // Act
            var response = await client.GetAsync(requestUrl);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("price for the cryptocurrency is not available.");
        }

        [Fact]
        public async Task SetCryptoCode_ReturnsOk_WithValidData()
        {
            // Arrange
            var newCryptoCode = "ETH";

            // 2. Inject Mocks into Test Environment
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptorSettings = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ICryptoSettingsService));
                    if (descriptorSettings != null)
                    {
                        services.Remove(descriptorSettings);
                    }

                    services.AddSingleton<ICryptoSettingsService>(_mockCryptoSettingsService.Object);
                });
            }).CreateClient();

            var requestContent = new StringContent(
                JsonConvert.SerializeObject(new { CryptoCode = newCryptoCode }),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await client.PostAsync("/crypto/cryptoCode", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain($"CryptoCode updated to {newCryptoCode.ToUpper()}.");
            _mockCryptoSettingsService.Verify(service => service.SetCryptoCode(newCryptoCode.ToUpper()), Times.Once);
        }

        [Fact]
        public async Task SetCryptoCode_ReturnsBadRequest_WithEmptyCryptoCode()
        {
            // Arrange
            var newCryptoCode = "";

            // 2. Inject Mocks into Test Environment
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptorSettings = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ICryptoSettingsService));
                    if (descriptorSettings != null)
                    {
                        services.Remove(descriptorSettings);
                    }

                    services.AddSingleton<ICryptoSettingsService>(_mockCryptoSettingsService.Object);
                });
            }).CreateClient();

            var requestContent = new StringContent(
                JsonConvert.SerializeObject(new { CryptoCode = newCryptoCode }),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await client.PostAsync("/crypto/cryptoCode", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("CryptoCode cannot be empty.");
            _mockCryptoSettingsService.Verify(service => service.SetCryptoCode(It.IsAny<string>()), Times.Never);
        }
    }
}
