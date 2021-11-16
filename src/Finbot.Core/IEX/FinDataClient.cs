namespace Finbot.Core.IEX;
using System;
using System.Threading.Tasks;
using Finbot.Core.IEX.Models;
using Microsoft.Extensions.Logging;
using RestSharp;

public class FinDataClient : IFinDataClient
{
    private readonly IRestClient client;

    private readonly ILogger logger;

    public FinDataClient(IRestClient client, ILogger logger)
    {
        this.logger = logger;
        this.client = client;
    }

    public async Task<ISecurityPrice> GetCryptoPriceAsync(string symbol)
    {
        var request = new RestRequest($"/crypto/{symbol}/price", DataFormat.Json);

        var result = await this.client.ExecuteAsync<CryptoPriceResult>(request);

        if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new ArgumentException($"Symbol {symbol} not found");
        }

        return result.Data;
    }

    public async Task<ISecurityPrice> GetPriceAsync(string symbol)
    {
        var request = new RestRequest($"/stock/{symbol}/quote", DataFormat.Json);

        var result = await this.client.ExecuteAsync<StockQuote>(request);

        if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new ArgumentException($"Symbol {symbol} not found");
        }

        return result.Data;
    }
}
