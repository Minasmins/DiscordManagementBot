using CoreRCON.Parsers.Standard;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using static System.Net.WebRequestMethods;

namespace DangerBotNamespace.Commands
{
    public class CSCommands : BaseCommandModule
    {
        [Command("startcs")]
        public async Task startcs(CommandContext context)
        {
            await context.RespondAsync("**Starting CS2 Server...**");
            var ActiveCSGOServer = CSGOGameServer.ServerInstance;
            ActiveCSGOServer.Context = context;
            await ActiveCSGOServer.StartSRCDS();
        }
        [Command("stopcs")]
        public async Task stopcs(CommandContext context)
        {
            await context.RespondAsync("**Stopping CS2 Server...**");
            var ActiveCSGOServer = CSGOGameServer.ServerInstance;
            ActiveCSGOServer.Context = context;
            await ActiveCSGOServer.StopSRCDS();
        }

        [Command("updatecs")]
        public async Task updatecs(CommandContext context)
        {
            await context.RespondAsync("**Updating CS2 Server...**");
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

        [Command("csstatus")]
        [Aliases("status")]
        public async Task csgostatus(CommandContext context)
        {
            SRCDSConfigJSON ServerConfig;
            var ActiveCSGOServer = CSGOGameServer.ServerInstance;
            var Json = string.Empty;
            using (var fs = System.IO.File.OpenRead("SRCDSConfig.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                Json = sr.ReadToEnd();
            ServerConfig = JsonConvert.DeserializeObject<SRCDSConfigJSON>(Json);
            var ServerLink = ServerConfig.SteamConnectionString;
            ActiveCSGOServer.Context = context;
            if (await ActiveCSGOServer.ConnectRCon(0, false))
            {
                var status = await ActiveCSGOServer.GetServerStatus();

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.DarkGreen,
                    Description = $"**Name:** {status.Hostname}\n" +
                    $"**Player:** {status.Humans}/{status.MaxPlayers}\n" +
                    $"**Map:** {status.Map}\n" +
                        $"**Version:** {status.Version}\n\n"
    ,
                    Title = "CS2 Server Status"
                };
                embed.AddField("Connection Link", ServerLink, true);
                await context.RespondAsync(embed);
            }
            else
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.DarkRed,
                    Description = $"**Error:** No Connection\n" +
                   "The server may not be started."
,
                    Title = "CS2 Server Status"
                };
                await context.RespondAsync(embed);
            }
        }

        [Command("randomizeteam")]
        [Aliases("random","rt", "scramble")]
        public async Task randomizeteam(CommandContext context)
        {
            var ActiveCSGOServer = CSGOGameServer.ServerInstance;
            ActiveCSGOServer.Context = context;
            var response = await ActiveCSGOServer.SendServerCommand("mp_scrambleteams");
            await context.RespondAsync("Mixing Teams...");
            await context.RespondAsync(response);
        }
    }
}