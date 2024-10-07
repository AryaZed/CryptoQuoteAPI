namespace CryptoQuoteAPI.Exceptions;

public class CurrencyNotAvailableException : Exception
{
    public CurrencyNotAvailableException(string currency)
        : base($"Currency '{currency}' is not available.")
    {
    }
}
