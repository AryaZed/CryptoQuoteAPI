namespace CryptoQuoteAPI.Configurations
{
    public class CryptoUpdateSettings
    {
        public string CryptoCode { get; set; }
        public List<string> Currencies { get; set; }
        public int UpdateIntervalMinutes { get; set; }
    }
}
