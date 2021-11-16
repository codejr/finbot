namespace Finbot.Core;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Finbot.Core.IEX;
using Finbot.Core.Portfolios;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class FinbotBrain : IFinbotBrain
{
    private readonly DiscordSocketClient client;

    private readonly CommandService commands;

    private readonly IFinDataClient finDataClient;

    private readonly ILogger<FinbotBrain> logger;

    private readonly IPortfolioService portfolioManager;

    private readonly IServiceProvider services;

    private readonly string token;

    public FinbotBrain(string token, IServiceProvider services)
    {
        this.token = token;
        this.services = services;
        this.logger = services.GetRequiredService<ILogger<FinbotBrain>>();
        this.portfolioManager = services.GetRequiredService<IPortfolioService>();
        this.finDataClient = services.GetRequiredService<IFinDataClient>();
        this.client = services.GetRequiredService<DiscordSocketClient>();
        this.commands = services.GetRequiredService<CommandService>();
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        this.client.Log += this.LogAsync;
        this.client.Ready += this.ReadyAsync;
        this.client.MessageReceived += this.MessageReceivedAsync;

        this.commands.CommandExecuted += this.CommandExecutedAsync;

        await this.commands.AddModulesAsync(typeof(FinbotBrain).Assembly, this.services);

        await this.client.LoginAsync(TokenType.Bot, this.token);
        await this.client.StartAsync();

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private Task LogAsync(LogMessage log)
    {
        switch (log.Severity)
        {
            case LogSeverity.Critical:
                this.logger.LogCritical(log.ToString());
                break;

            case LogSeverity.Error:
                this.logger.LogError(log.ToString());
                break;

            case LogSeverity.Warning:
                this.logger.LogWarning(log.ToString());
                break;

            case LogSeverity.Info:
                this.logger.LogInformation(log.ToString());
                break;
            case LogSeverity.Verbose:
                break;
            case LogSeverity.Debug:
                break;
            default:
                this.logger.LogDebug(log.ToString());
                break;
        }

        return Task.CompletedTask;
    }

    private Task ReadyAsync()
    {
        this.logger.LogInformation($"{this.client.CurrentUser} is connected!");

        return Task.CompletedTask;
    }

    private string GetUsage(CommandInfo command)
    {
        var usage = new StringBuilder();
        usage.Append($"!{command.Name}");

        foreach (var param in command.Parameters)
        {
            usage.Append($" [{param.Name}");
            if (param.IsOptional)
            {
                usage.Append(":optional");
            }

            usage.Append(']');
        }

        return usage.ToString();
    }

    public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        if (!command.IsSpecified)
        {
            return;
        }

        if (result.IsSuccess)
        {
            return;
        }

        if (result.Error != null)
        {
            switch (result.Error)
            {
                case CommandError.ParseFailed:
                    await context.Channel.SendMessageAsync(
                        $"Cannot parse command. Possible improper usage.\r\nUsage: `{this.GetUsage(command.Value)}`");
                    break;

                case CommandError.BadArgCount:
                    await context.Channel.SendMessageAsync(
                        $"Missing command arguments.\r\nUsage: `{this.GetUsage(command.Value)}`");
                    break;
                case CommandError.UnknownCommand:
                    break;
                case CommandError.ObjectNotFound:
                    break;
                case CommandError.MultipleMatches:
                    break;
                case CommandError.UnmetPrecondition:
                    break;
                case CommandError.Exception:
                    break;
                case CommandError.Unsuccessful:
                    break;
                default:
                    await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
                    break;
            }
        }

        if (result is ExecuteResult eResult)
        {
            this.logger.LogError(eResult.Exception.ToString());
        }
    }

    private async Task MessageReceivedAsync(SocketMessage rawMessage)
    {
        this.logger.LogDebug($"Received Message: {rawMessage.Content}");

        var argPos = 0;

        // Ignore non-user messages
        if (rawMessage is not SocketUserMessage message ||
            message.Source != MessageSource.User ||
            !message.HasCharPrefix('!', ref argPos))
        {
            return;
        }

        var context = new SocketCommandContext(this.client, message);
        await this.commands.ExecuteAsync(context, argPos, this.services);
    }
}
