﻿using Discord.Commands;
using Finbot.Core.IEX;
using System.Threading.Tasks;

namespace Finbot.Core.Modules
{
    [Group("Pricing")]
    public class PricingModule : ModuleBase<SocketCommandContext>
    {
        private readonly IFinDataClient finDataClient;

        public PricingModule(IFinDataClient finDataClient)
        {
            this.finDataClient = finDataClient;
        }

        private string FormatPrice(string symbol, decimal? price) => $":moneybag: Price for **{symbol}** at **{price?.ToString("C")}**";

        [Command("price")]
        [Summary("Checks the price of a stock.")]
        public async Task StockPriceAsync(string symbol)
        {
            var priceResponce = await finDataClient.GetPriceAsync(symbol);

            await ReplyAsync(FormatPrice(priceResponce.Symbol, priceResponce.Price));
        }

        [Command("pricecrypto")]
        [Summary("Checks the price of a cryptocurrency.")]
        public async Task CryptoPriceAsync(string symbol)
        {
            var priceResponce = await finDataClient.GetCryptoPriceAsync(symbol);

            await ReplyAsync(FormatPrice(priceResponce.Symbol, priceResponce.Price));
        }
    }
}