using CryptoQuoteAPI.Services.CoinMarketCap;

public class CryptoSettingsService : ICryptoSettingsService
{
    private string _cryptoCode;
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    public CryptoSettingsService()
    {
        _cryptoCode = "BTC";
    }

    public string GetCryptoCode()
    {
        _lock.EnterReadLock();
        try
        {
            return _cryptoCode;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void SetCryptoCode(string cryptoCode)
    {
        _lock.EnterWriteLock();
        try
        {
            _cryptoCode = cryptoCode.ToUpper();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}
