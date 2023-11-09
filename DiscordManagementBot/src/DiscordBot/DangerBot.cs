using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Newtonsoft.Json;
using DangerBotNamespace.Commands;
using DSharpPlus.Entities;

namespace DangerBotNamespace
{
    public class DangerBot
    {
         public DiscordClient DangerBotClient {get; private set;}
         public InteractivityExtension Interactivity {get; private set;}
         public CommandsNextExtension Commands {get; private set;}
        private CSGOGameServer ActiveCSGOServer { get; set;}
        public async Task RunAsync()
         {
            var Json = string.Empty;
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
            Json = await sr.ReadToEndAsync();
            var configJson = JsonConvert.DeserializeObject<ConfigJSON>(Json);

            var DiscordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            DangerBotClient = new DiscordClient(DiscordConfig);
            DangerBotClient.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(2)
            });


            var CommandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new String[] { configJson.Prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false
            };

            Commands = DangerBotClient.UseCommandsNext(CommandsConfig);
            Commands.RegisterCommands<BasicCommands>();
            Commands.RegisterCommands<CSCommands>();
            //Hier weiter Command Klassen Registrieren

            //Neuen CSGOServer initieren um den dann immer weiter zu geben
            ActiveCSGOServer = CSGOGameServer.ServerInstance;
            


            await DangerBotClient.ConnectAsync();
            await Task.Delay(-1);
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}