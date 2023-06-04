using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DangerBotNamespace.Commands
{
    public class CSGOCommands : BaseCommandModule
    {
        [Command("startcsgo")]
        public async Task startcsgo(CommandContext context)
        {
            await context.RespondAsync("CSGO Starten");
            var ActiveCSGOServer = CSGOGameServer.ServerInstance;
            await ActiveCSGOServer.StartSRCDS();
            await context.RespondAsync("CSGO Start initiert");
        }
    }
}