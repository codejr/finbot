using Discord.Commands;
using Finbot.Core.IEX;
using System.Threading.Tasks;

namespace Finbot.Core.Modules
{
    public class PricingModule : ModuleBase<SocketCommandContext>
    {
        private readonly IFinDataClient finDataClient;

        public PricingModule(IFinDataClient finDataClient)
        {
            this.finDataClient = finDataClient;
        }

        private string FormatPrice(string symbol, decimal? price) => $":moneybag: Price for **{symbol}** at **{price?.ToString("C")}**";

        [Command("price")]
        [Summary("Checks the price of a stock. E.g. !price TSLA")]
        [Remarks("`!price [Symbol]`")]
        public async Task StockPriceAsync(string symbol)
        {
            var priceResponce = await finDataClient.GetPriceAsync(symbol);

            await ReplyAsync(FormatPrice(priceResponce.Symbol, priceResponce.Price));
        }

        [Command("pricecrypto")]
        [Summary("Checks the price of a cryptocurrency. Note crypto listings usually include currency at the end. E.g. !pricecrypto BTCUSD")]
        [Remarks("`!pricecrypto [Symbol]`")]
        public async Task CryptoPriceAsync(string symbol)
        {
            var priceResponce = await finDataClient.GetCryptoPriceAsync(symbol);

            await ReplyAsync(FormatPrice(priceResponce.Symbol, priceResponce.Price));
        }
    }
}
