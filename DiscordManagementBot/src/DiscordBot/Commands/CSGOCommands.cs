using CoreRCON.Parsers.Standard;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DangerBotNamespace.Commands
{
    public class CSGOCommands : BaseCommandModule
    {
        [Command("startcsgo")]
        public async Task startcsgo(CommandContext context)
        {
            await context.RespondAsync("CSGO Starten");
            var ActiveCSGOServer = CSGOGameServer.ServerInstance;
            ActiveCSGOServer.Context = context;
            await ActiveCSGOServer.StartSRCDS();
            Thread.Sleep(15000);
            await ActiveCSGOServer.ConnectRCon();
        }
        [Command("stopcsgo")]
        public async Task stopcsgo(CommandContext context)
        {
            await context.RespondAsync("CSGO Stoppen");
            var ActiveCSGOServer = CSGOGameServer.ServerInstance;
            ActiveCSGOServer.Context = context;
            await ActiveCSGOServer.StopSRCDS();
        }

        [Command("updatecsgo")]
        public async Task updatecsgo(CommandContext context)
        {
            await context.RespondAsync("CSGO updaten");
            var ActiveCSGOServer = CSGOGameServer.ServerInstance;
            ActiveCSGOServer.Context = context;
            await ActiveCSGOServer.UpdateSRCDS();
        }

        [Command("rcon")]
        [Aliases("rc")]
        public async Task rcon(CommandContext context,[RemainingText] string Command)
        {
            var ActiveCSGOServer = CSGOGameServer.ServerInstance;
            ActiveCSGOServer.Context = context;
            var response = await ActiveCSGOServer.SendServerCommand(Command);
            await context.RespondAsync(response);
        }

        [Command("csgostatus")]
        [Aliases("status")]
        public async Task csgostatus(CommandContext context)
        {
            var ActiveCSGOServer = CSGOGameServer.ServerInstance;
            ActiveCSGOServer.Context = context;
            try
            {
                var status = await ActiveCSGOServer.GetServerStatus();

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.DarkGreen,
                    Description = $"**Name:** {status.Hostname}\n" +
                        $"**Players:** {status.Humans} / 20\n" +
                        $"**Current Map:** {status.Map}\n" +
                        $"**Bots:** {status.Bots}\n" +
                        $"**Version:** {status.Version}\n\n"
    ,
                    Title = "CSGO Server Status"
                };
                await context.RespondAsync(embed);
            }
            catch (Exception ex)
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.DarkRed,
                    Description = $"**Name:** {ex.Message}\n" +
                        $"**Players:** {ex.Message} / 20\n" +
                        $"**Current Map:** {ex.Message}\n" +
                    $"**Bots:** {ex.Message}\n" +
                        $"**Version:** {ex.Message}\n\n" +
                        "The server may not be started."
    ,
                    Title = "CSGO Server Status"
                };
                await context.RespondAsync(embed);
                //await context.RespondAsync($"Error: {ex.Message} \nThe server may not be started.");
            }
            
        }

    }
}