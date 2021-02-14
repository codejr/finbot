using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finbot.Core.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService commandService;

        public HelpModule(CommandService commandService)
        {
            this.commandService = commandService;
        }

        [Command("help")]
        public async Task Help()
        {
            var commands = commandService.Commands.ToList();
            var embedBuilder = new EmbedBuilder().WithTitle("Help");

            foreach (var command in commands)
            {
                var embedFieldText = command.Summary ?? "No description available\n";

                embedBuilder.AddField(command.Name, embedFieldText);
            }

            await ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
        }
    }
}
