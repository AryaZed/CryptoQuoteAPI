namespace CryptoQuoteAPI.Exceptions;

public class CryptocurrencyNotFoundException : Exception
{
    public string CryptoCode { get; }

    public CryptocurrencyNotFoundException(string cryptoCode)
        : base($"Cryptocurrency '{cryptoCode}' was not found.")
    {
        CryptoCode = cryptoCode;
    }
}