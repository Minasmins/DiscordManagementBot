
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;   
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Bson;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using CoreRCON;
using CoreRCON.Parsers.Standard;
using System.Net;
using System.Threading.Channels;
using System.Linq.Expressions;

namespace DangerBotNamespace
{
    public sealed class CSGOGameServer
    {
        private SRCDSConfigJSON SRCDSConfig {get; set;}
        public Process CS2Process { get; set; }
        public Process SteamCMDProcess { get; set; }
        public Boolean ServerNeedsUpdate { get; private set; }
        public string processPriority { get; private set; }
        public CommandContext Context { get; set; }
        public RCON RconConnection { get; set; }
        private static readonly CSGOGameServer Instance = new CSGOGameServer();

        string OutputDataxline;
        bool IsKillingServerProcess;

        public static CSGOGameServer ServerInstance
        {
            get{
                return Instance;
            }
        }

        private CSGOGameServer()
        {
            var Json = string.Empty;
            using (var fs = File.OpenRead("SRCDSConfig.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
            Json = sr.ReadToEnd();
            SRCDSConfig = JsonConvert.DeserializeObject<SRCDSConfigJSON>(Json);
        }

       

        public async Task<bool> StartSRCDS()
        {
            IsKillingServerProcess = true;
            CS2Process = new Process();
            CS2Process.EnableRaisingEvents = true;
            CS2Process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
            CS2Process.ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);
            CS2Process.Exited += new EventHandler(process_Exited);

            CS2Process.StartInfo.FileName = SRCDSConfig.srcdsPath + "\\CS2.exe";
            CS2Process.StartInfo.Arguments = SRCDSConfig.srcdsStartArguments;
            CS2Process.StartInfo.UseShellExecute = false;
            CS2Process.StartInfo.RedirectStandardError = true;
            CS2Process.StartInfo.RedirectStandardOutput = true;
            CS2Process.StartInfo.RedirectStandardInput = true;

            CS2Process.Start();
            CS2Process.BeginErrorReadLine();
            CS2Process.BeginOutputReadLine();

            switch (SRCDSConfig.srcdsProcessPriorityClass)
            {
                case 1:
                    CS2Process.PriorityClass = ProcessPriorityClass.Idle;
                    processPriority = "Idle";
                    break;
                case 2:
                    CS2Process.PriorityClass = ProcessPriorityClass.BelowNormal;
                    processPriority = "Below Normal";
                    break;
                case 3:
                    CS2Process.PriorityClass = ProcessPriorityClass.Normal;
                    processPriority = "Normal";
                    break;
                case 4:
                    CS2Process.PriorityClass = ProcessPriorityClass.AboveNormal;
                    processPriority = "Above Normal";
                    break;
                case 5:
                    CS2Process.PriorityClass = ProcessPriorityClass.High;
                    processPriority = "High";
                    break;
                case 6:
                    CS2Process.PriorityClass = ProcessPriorityClass.RealTime;
                    processPriority = "RealTime";
                    break;
                default:
                    CS2Process.PriorityClass = ProcessPriorityClass.Normal;
                    processPriority = "Normal";
                    break;
            }
            CS2Process.StartInfo.UseShellExecute = false;
            CS2Process.StartInfo.RedirectStandardError = true;
            CS2Process.StartInfo.RedirectStandardOutput = true;
            CS2Process.StartInfo.RedirectStandardInput = true;

            return await ConnectRCon(15000, true);
        }

        private static void process_OutputDataReceived(object sendingProcess,DataReceivedEventArgs outLine)
        {
            string line;
            if (outLine.Data == null)
            { return; }
            line = (outLine.Data.ToString());
            if (line == Instance.OutputDataxline)
                {
                    return;
                };
            
            Console.WriteLine(line);
            Instance.OutputDataxline = line;
        }

        private static void process_ErrorDataReceived(object sendingProcess, DataReceivedEventArgs outLine)
        {
            string line;
            try
            {
                if(outLine.Data == null)
                { return; }
                line = (outLine.Data.ToString());
            }
            catch (Exception ex) { line = ex.Message; }

            Console.WriteLine(line);
        }

        private static void process_Exited(object sender, System.EventArgs e)
        {
            string line;
            line = "CS2 process exited";
            Console.WriteLine(line);
            if (line.Length == 0)
            { return; }
            Instance.Context.Channel.SendMessageAsync(line).Wait();
        }

        public async Task<string> StopSRCDS()
        {
            Instance.IsKillingServerProcess = true;
            if (CS2Process != null)
            {
                return await SendServerCommand("quit");
            }
            else
            {
                Instance.Context.Channel.SendMessageAsync("CS2 server already offline").Wait();
                return ("CS2 server offline");
            }
            
        }

        public async Task<bool> UpdateSRCDS()
        {
            await StopSRCDS();
            SteamCMDProcess = new Process();
            SteamCMDProcess.EnableRaisingEvents = true;
            SteamCMDProcess.OutputDataReceived += new DataReceivedEventHandler(Update_OutputDataReceived);
            SteamCMDProcess.ErrorDataReceived += new DataReceivedEventHandler(Update_ErrorDataReceived);
            SteamCMDProcess.Exited += Update_Exited;

            SteamCMDProcess.StartInfo.FileName = SRCDSConfig.SteamCMDPath + "\\steamcmd.exe";
            SteamCMDProcess.StartInfo.Arguments = SRCDSConfig.SteamCMDArguments;
            SteamCMDProcess.StartInfo.UseShellExecute = false;
            SteamCMDProcess.StartInfo.RedirectStandardError = true;
            SteamCMDProcess.StartInfo.RedirectStandardOutput = true;
            SteamCMDProcess.StartInfo.RedirectStandardInput = true;

            SteamCMDProcess.Start();
            SteamCMDProcess.BeginErrorReadLine();
            SteamCMDProcess.BeginOutputReadLine();
            SteamCMDProcess.WaitForExit();
            //await Context.Channel.SendMessageAsync(UpdateText);
            return await StartSRCDS();
        }

        void Update_Exited(object sender, EventArgs e)
        {
            Console.WriteLine("The srcds process has been exited/killed");
            Context.Channel.SendMessageAsync("The cs2 process has been exited/killed");
        }

        void Update_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                Context.Channel.SendMessageAsync(e.Data);
            }
            catch(Exception ex) { Console.WriteLine($"Error while recieving Error-Data: {ex.Message}"); }
            
        }

        void Update_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                if (e.Data != null) 
                {
                    if (e.Data.Contains("verifying install") || e.Data.Contains("downloading, progress"))
                    {
                        Console.WriteLine(e.Data);
                    }
                    else
                    {
                        Console.WriteLine(e.Data);
                        Context.Channel.SendMessageAsync(e.Data);
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error while recieving Error-Data: {ex.Message}"); }
        }

        public async Task<bool> ConnectRCon(int DelayToConnect, bool SendInfoMsg)
        {
            Console.WriteLine($"Trying to connect...");
            Thread.Sleep(DelayToConnect);
            try
            {
                RconConnection = new RCON(IPAddress.Parse(SRCDSConfig.ServerIP), SRCDSConfig.ServerPort, SRCDSConfig.RconPassword, 2000);
                await RconConnection.ConnectAsync();
                if (SendInfoMsg)
                {
                    Status status = await RconConnection.SendCommandAsync<Status>("status");
                    Console.WriteLine($"Server online / RCON-Connection established to: {status.Hostname}");
                    Context.Channel.SendMessageAsync($"Server online / RCON-Connection established to: {status.Hostname}").Wait();
                }
                return true;
            }
            catch 
            {
                Console.WriteLine("Connection to CS2-Server could not be established!");
                return false;
            }
        }

        public async Task<string> SendServerCommand(string command)
        {
            await ConnectRCon(0,false);
            return await RconConnection.SendCommandAsync(command);
        }

        public async Task<Status> GetServerStatus()
        {
                Status status = await RconConnection.SendCommandAsync<Status>("status");
                return status;
        }
    }
}