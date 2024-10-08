namespace CryptoQuoteAPI.Services.CoinMarketCap
{
    public interface ICryptoSettingsService
    {
        string GetCryptoCode();
        void SetCryptoCode(string cryptoCode);
    }
}
