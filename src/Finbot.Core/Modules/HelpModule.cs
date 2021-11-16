namespace Finbot.Core.Modules;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

public class HelpModule : ModuleBase<SocketCommandContext>
{
    private readonly CommandService commandService;

    public HelpModule(CommandService commandService) => this.commandService = commandService;

    [Command("help")]
    public async Task Help()
    {
        var commands = this.commandService.Commands.ToList();
        var embedBuilder = new EmbedBuilder().WithTitle("Help").WithColor(Color.Green);

        foreach (var command in commands)
        {
            var embedFieldText = command.Summary ?? "No description available\n";

            embedBuilder.AddField(command.Name, embedFieldText);
        }

        await this.ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
    }
}
