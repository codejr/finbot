namespace Finbot.Core.Modules;
using System.Threading.Tasks;
using Discord.Commands;
using Finbot.Core.IEX;

public class PricingModule : ModuleBase<SocketCommandContext>
{
    private readonly IFinDataClient finDataClient;

    public PricingModule(IFinDataClient finDataClient) => this.finDataClient = finDataClient;

    private static string FormatPrice(string symbol, decimal? price, decimal quantity) => $":moneybag: Price for {quantity} of **{symbol}** at **${price * quantity:0.00#####}**";

    [Command("price")]
    [Alias("p")]
    [Summary("Checks the price of a stock. E.g. !price TSLA")]
    [Remarks("`!price [Symbol]`")]
    public async Task StockPriceAsync(string symbol, int quantity = 1)
    {
        var priceResponce = await this.finDataClient.GetPriceAsync(symbol);

        await this.ReplyAsync(FormatPrice(priceResponce.Symbol, priceResponce.Price, quantity));
    }

    [Command("pricecrypto")]
    [Alias("pc")]
    [Summary("Checks the price of a cryptocurrency. Note crypto listings usually include currency at the end. E.g. !pricecrypto BTCUSD")]
    [Remarks("`!pricecrypto [Symbol]`")]
    public async Task CryptoPriceAsync(string symbol, decimal quantity = 1.0m)
    {
        var priceResponce = await this.finDataClient.GetCryptoPriceAsync(symbol);

        await this.ReplyAsync(FormatPrice(priceResponce.Symbol, priceResponce.Price, quantity));
    }
}
