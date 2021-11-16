namespace Finbot.Service;
using System.Threading;
using System.Threading.Tasks;
using Finbot.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
        this.logger.LogInformation("Starting service...");

        _ = this.bot.RunAsync(cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Stopping");

        return Task.CompletedTask;
    }
}
