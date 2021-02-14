using System;
using System.Threading.Tasks;
using Finbot.Core;
using Finbot.Core.IEX;
using Finbot.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace finbot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args);

            await host.RunConsoleAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IPortfolioManager, PortfolioManager>();

                services.AddTransient<IFinDataClient, FinDataClient>(
                    serviceManager =>
                    {
                        var uri = Environment.GetEnvironmentVariable("iex.uri");
                        var restCLient = new RestClient(uri);
                        restCLient.Authenticator = new TokenAuthenticator(Environment.GetEnvironmentVariable("iex.token"));
                        return new FinDataClient(restCLient, serviceManager.GetService<ILogger>());
                    });

                services.AddTransient<IFinbotBrain, FinbotBrain>(
                    serviceManager =>
                    {
                        var token = Environment.GetEnvironmentVariable("discord.token");
                        var finCLient = serviceManager.GetService<IFinDataClient>();
                        var logger = serviceManager.GetService<ILogger<FinbotBrain>>();
                        var portfolioManager = serviceManager.GetService<IPortfolioManager>();

                        return new FinbotBrain(token, finCLient, logger, portfolioManager);
                    });

                services.AddHostedService<FinbotService>();
            });
    }
}