namespace CryptoQuoteAPI.Exceptions
{
    public class ExchangeRateApiException : Exception
    {
        public int StatusCode { get; }

        public ExchangeRateApiException(int statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
