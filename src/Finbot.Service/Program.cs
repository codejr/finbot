using System;
using System.Reflection;
using System.Threading.Tasks;
using Finbot.Core;
using Finbot.Core.IEX;
using Finbot.Service;
using Microsoft.Extensions.Configuration;
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
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                var env = hostContext.HostingEnvironment;

                config.AddEnvironmentVariables();

                if (env.IsDevelopment())
                {
                    var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                    if (appAssembly != null)
                    {
                        config.AddUserSecrets(appAssembly, optional: true);
                    }
                }
            })
            .ConfigureServices((hostContext, services) =>
            {
                var config = hostContext.Configuration;

                services.AddSingleton<IPortfolioManager, PortfolioManager>();

                services.AddTransient<IFinDataClient, FinDataClient>(
                    serviceManager =>
                    {
                        var restCLient = new RestClient(config.GetValue<string>("iex:uri"))
                        {
                            Authenticator = new TokenAuthenticator(config.GetValue<string>("iex:token"))
                        };

                        return new FinDataClient(restCLient, serviceManager.GetService<ILogger>());
                    });

                services.AddTransient<IFinbotBrain, FinbotBrain>(
                    serviceManager =>
                    {
                        var token = config.GetValue<string>("discord:token");
                        var finCLient = serviceManager.GetService<IFinDataClient>();
                        var logger = serviceManager.GetService<ILogger<FinbotBrain>>();
                        var portfolioManager = serviceManager.GetService<IPortfolioManager>();

                        return new FinbotBrain(token, finCLient, logger, portfolioManager);
                    });

                services.AddHostedService<FinbotService>();
            });
    }
}