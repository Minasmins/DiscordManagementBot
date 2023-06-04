using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DangerBotNamespace.Commands
{
    public class BasicCommands : BaseCommandModule
    {
        [Command("help")]
        public async Task help(CommandContext context)
        {
            await context.RespondAsync("Work in Progress");
            
        }
    }
}