namespace CryptoQuoteAPI.Exceptions
{
    public class ExchangeRateNotFoundException : Exception
    {
        public string Currency { get; }

        public ExchangeRateNotFoundException(string currency)
            : base($"Exchange rate for currency '{currency}' was not found.")
        {
            Currency = currency;
        }
    }
}
