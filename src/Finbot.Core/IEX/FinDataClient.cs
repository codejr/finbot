using Finbot.Core.IEX.Models;
using Microsoft.Extensions.Logging;
using RestSharp;
using System.Threading.Tasks;

namespace Finbot.Core.IEX
{
    public class FinDataClient : IFinDataClient
    {
        private IRestClient client;

        private ILogger logger;

        public FinDataClient(IRestClient client, ILogger logger)
        {
            this.logger = logger;
            this.client = client;
        }

        public async Task<ISecurityPrice> GetCryptoPriceAsync(string symbol)
        {
            var request = new RestRequest($"/crypto/{symbol}/price", DataFormat.Json);

            return await client.GetAsync<CryptoPriceResult>(request);
        }

        public async Task<ISecurityPrice> GetPriceAsync(string symbol)
        {
            var request = new RestRequest($"/stock/{symbol}/quote", DataFormat.Json);

            return await client.GetAsync<StockQuote>(request);
        }
    }
}
