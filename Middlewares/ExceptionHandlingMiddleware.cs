using CryptoQuoteAPI.Exceptions;
using System.Text.Json;

namespace CryptoQuoteAPI.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                context.Response.ContentType = "application/json";

                var (statusCode, errorMessage) = GetErrorResponse(ex);

                context.Response.StatusCode = statusCode;

                var errorResponse = new { Message = errorMessage };

                var errorJson = JsonSerializer.Serialize(errorResponse);

                await context.Response.WriteAsync(errorJson);
            }
        }

        private (int StatusCode, string ErrorMessage) GetErrorResponse(Exception exception)
        {
            return exception switch
            {
                CryptocurrencyNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                CurrencyNotAvailableException => (StatusCodes.Status400BadRequest, exception.Message),
                ExchangeRateApiException ex => (ex.StatusCode == 0 ? StatusCodes.Status503ServiceUnavailable : ex.StatusCode, ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };
        }
    }
}
