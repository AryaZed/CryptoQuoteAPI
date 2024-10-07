using CryptoQuoteAPI.Services.CoinMarketCap;
using CryptoQuoteAPI.Services.ExchangeRate;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using CryptoQuoteAPI.Exceptions;
using CryptoQuoteAPI.Models.CoinMarketCap;

namespace CryptoQuoteAPI.Endpoints
{
    /// <summary>
    /// Defines the /crypto/{cryptoCode} endpoint.
    /// </summary>
    public class CryptoEndpoint
    {
        private readonly ILogger<CryptoEndpoint> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoEndpoint"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public CryptoEndpoint(ILogger<CryptoEndpoint> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles the incoming request to get cryptocurrency quotes.
        /// </summary>
        /// <param name="request">The request containing cryptocurrency code and target currencies.</param>
        /// <param name="cryptoService">Service to fetch cryptocurrency prices.</param>
        /// <param name="exchangeRateService">Service to fetch exchange rates.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A response containing cryptocurrency prices in target currencies.</returns>
        public async Task<IResult> HandleAsync(
            GetCryptoQuoteRequest request,
            ICryptoService cryptoService,
            IExchangeRateService exchangeRateService,
            CancellationToken cancellationToken = default)
        {
            // Validate the request using Data Annotations
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            if (!isValid)
            {
                var errors = validationResults.Select(vr => vr.ErrorMessage).ToList();
                return Results.BadRequest(new { Messages = errors });
            }

            var targetCurrencies = request.Currencies != null && request.Currencies.Any()
                ? request.Currencies.Select(c => c.ToUpper()).Distinct().ToList()
                : new List<string> { "USD", "EUR", "BRL", "GBP", "AUD" };

            try
            {
                // Fetch cryptocurrency prices
                var cryptoPrices = await cryptoService.GetCryptoPricesAsync(request.CryptoCode.ToUpper(), targetCurrencies, cancellationToken);

                // Fetch exchange rates
                //var exchangeRates = await exchangeRateService.GetExchangeRatesAsync("USD", targetCurrencies, cancellationToken);

                if (!cryptoPrices.Any())
                {
                    _logger.LogWarning("price for cryptocurrency '{CryptoCode}' is not available.", request.CryptoCode);
                    return Results.BadRequest(new { Message = "price for the cryptocurrency is not available." });
                }

                // Combine crypto price with exchange rates to get prices in target currencies
                var finalResults = targetCurrencies.Select(currency =>
                {
                    var exchangeRateResult = cryptoPrices.FirstOrDefault(er => er.Currency == currency);

                    if (exchangeRateResult != null && exchangeRateResult.Price.HasValue)
                    {
                        return new CryptoQuoteResponse
                        {
                            Currency = currency,
                            Price = exchangeRateResult.Price
                        };
                    }
                    else
                    {
                        return new CryptoQuoteResponse
                        {
                            Currency = currency,
                            Price = null,
                            Error = $"Exchange rate for '{currency}' is not available."
                        };
                    }
                });

                return Results.Ok(finalResults);
            }
            catch (CryptocurrencyNotFoundException cnfEx)
            {
                _logger.LogError(cnfEx, "Cryptocurrency '{CryptoCode}' not found.", request.CryptoCode);
                return Results.NotFound(new { Message = $"Cryptocurrency '{request.CryptoCode}' was not found." });
            }
            catch (ExchangeRateApiException eraEx)
            {
                _logger.LogError(eraEx, "ExchangeRatesAPI error: {Message}", eraEx.Message);
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while processing the crypto quote request.");
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/crypto/{cryptoCode}", async (
                string cryptoCode,
                [FromQuery] string[]? currencies,
                ICryptoService cryptoService,
                IExchangeRateService exchangeRateService,
                CancellationToken cancellationToken) =>
            {
                var request = new GetCryptoQuoteRequest
                {
                    CryptoCode = cryptoCode,
                    Currencies = currencies
                };

                return await HandleAsync(request, cryptoService, exchangeRateService, cancellationToken);
            })
            .WithName("GetCryptoQuote")
            .Produces<IEnumerable<CryptoQuoteResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status503ServiceUnavailable)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags("Crypto");

            app.MapGet("/crypto/cryptoCode", (ICryptoSettingsService cryptoSettingsService) =>
            {
                var cryptoCode = cryptoSettingsService.GetCryptoCode();
                return Results.Ok(new { CryptoCode = cryptoCode });
            })
            .WithName("GetCryptoCode")
            .Produces(StatusCodes.Status200OK);
            
            app.MapPost("/crypto/cryptoCode", async (HttpContext http, ICryptoSettingsService cryptoSettingsService) =>
            {
                var request = await http.Request.ReadFromJsonAsync<CryptoCodeUpdateRequest>();
                if (request == null || string.IsNullOrWhiteSpace(request.CryptoCode))
                {
                    return Results.BadRequest(new { Message = "CryptoCode cannot be empty." });
                }

                cryptoSettingsService.SetCryptoCode(request.CryptoCode);
                return Results.Ok(new { Message = $"CryptoCode updated to {request.CryptoCode.ToUpper()}." });
            })
            .WithName("SetCryptoCode")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}
