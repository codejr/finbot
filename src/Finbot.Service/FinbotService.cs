using Finbot.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Finbot.Service
{
    public class FinbotService : IHostedService
    {
        private readonly ILogger<FinbotService> logger;

        private readonly IFinbotBrain bot;

        public FinbotService(ILogger<FinbotService> logger, IFinbotBrain bot)
        {
            this.logger = logger;
            this.bot = bot;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting service...");

            _ = bot.RunAsync(cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping");

            return Task.CompletedTask;
        }
    }
}
