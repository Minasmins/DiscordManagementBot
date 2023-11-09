using CoreRCON.Parsers.Standard;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System.Text;

namespace DangerBotNamespace.Commands
{
    public class BasicCommands : BaseCommandModule
    {
        [Command("help")]
        public async Task help(CommandContext context)
        {
            var Json = string.Empty;
            using (var fs = File.OpenRead("Config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                Json = sr.ReadToEnd();
            var Config = JsonConvert.DeserializeObject<ConfigJSON>(Json);
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {

            
            Color = DiscordColor.Blue,
                Description = 
                $"**{Config.Prefix}help:** Shows all available Commands\n" + 
                $"**{Config.Prefix}startcs:** Starts the CS2-Server\n" + 
                $"**{Config.Prefix}stopcs:** Stops the CS2-Server\n" + 
                $"**{Config.Prefix}updatecs:** Stops, updates and starts the CS2-Server\n" + 
                $"**{Config.Prefix}rcon [Command]:** Executes [Command] directly on the server \n"+
                $"**{Config.Prefix}random:** Scrambles the teams and restarts the game\n",
                Title = "DangerBot - Help"
            };
            await context.RespondAsync(embed);

        }
    }
}