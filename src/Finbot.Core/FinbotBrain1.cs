using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Finbot.Core.IEX;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Finbot.Core.Portfolios;
using System.Text;

namespace Finbot.Core
{
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
            this.client.Log += LogAsync;
            this.client.Ready += ReadyAsync;
            this.client.MessageReceived += MessageReceivedAsync;

            this.commands.CommandExecuted += CommandExecutedAsync;
            
            await this.commands.AddModulesAsync(typeof(FinbotBrain).Assembly, services);

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite, cancellationToken);
        }

        private Task LogAsync(LogMessage log)
        {
            switch (log.Severity)
            {
                case LogSeverity.Critical:
                    logger.LogCritical(log.ToString());
                    break;

                case LogSeverity.Error:
                    logger.LogError(log.ToString());
                    break;

                case LogSeverity.Warning:
                    logger.LogWarning(log.ToString());
                    break;

                case LogSeverity.Info:
                    logger.LogInformation(log.ToString());
                    break;

                default:
                    logger.LogDebug(log.ToString());
                    break;
            }

            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            logger.LogInformation($"{client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        private string GetUsage(CommandInfo command) 
        {
            var usage = new StringBuilder();
            usage.Append($"!{command.Name}");
            
            foreach(var param in command.Parameters) 
            {
                usage.Append($" [{param.Name}");
                if (param.IsOptional) usage.Append(":optional");
                usage.Append("]");
            }

            return usage.ToString();
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
                return;

            if (result.IsSuccess)
                return;

            if (result.Error != null)
            {
                switch (result.Error)
                {
                    case CommandError.ParseFailed:
                        await context.Channel.SendMessageAsync(
                            $"Cannot parse command. Possible improper usage.\r\nUsage: `{GetUsage(command.Value)}`");
                        break;

                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync(
                            $"Missing command arguments.\r\nUsage: `{GetUsage(command.Value)}`");
                        break;

                    default:
                        await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
                        break;
                }
            }

            if (result is ExecuteResult eResult)
            {
                logger.LogError(eResult.Exception.ToString());
            }
        }

        private async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            logger.LogDebug($"Received Message: {rawMessage.Content}");

            int argPos = 0;

            // Ignore non-user messages
            if (rawMessage is not SocketUserMessage message ||
                message.Source != MessageSource.User ||
                !message.HasCharPrefix('!', ref argPos))
            {
                return;
            }

            var context = new SocketCommandContext(client, message);
            await commands.ExecuteAsync(context, argPos, services);
        }
    }
}
