using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CryptoQuoteAPI.Hubs
{
    public class CryptoHub : Hub
    {
        public async Task SendCryptoUpdate(string cryptoCode, string currency, decimal price)
        {
            await Clients.All.SendAsync("ReceiveCryptoUpdate", cryptoCode, currency, price);
        }
    }
}