
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

namespace DangerBotNamespace
{
    public sealed class CSGOGameServer
    {
        private SRCDSConfigJSON SRCDSConfig {get; set;}
        public Process CSGOProcess { get; set; }
        public Process SteamCMDProcess { get; set; }
        public Boolean ServerNeedsUpdate { get; private set; }
        public CommandContext Context { get; set; }
        public RCON RconConnection { get; set; }
        private static readonly CSGOGameServer Instance = new CSGOGameServer();

        private string UpdateText;

        public static CSGOGameServer ServerInstance
        {
            get{
                return Instance;
            }
        }

        private CSGOGameServer()
        {
            //StartSRCDS(Guild);
            var Json = string.Empty;
            using (var fs = File.OpenRead("SRCDSConfig.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
            Json = sr.ReadToEnd();
            SRCDSConfig = JsonConvert.DeserializeObject<SRCDSConfigJSON>(Json);
            ServerNeedsUpdate = false;
            ConnectRCon();


        }

       

        public async Task StartSRCDS()
        {
            CSGOProcess = new System.Diagnostics.Process();
            CSGOProcess.EnableRaisingEvents = true;
            //CSGOProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_OutputDataReceived);
            //CSGOProcess.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_ErrorDataReceived);
            //CSGOProcess.Exited += process_Exited;

            CSGOProcess.StartInfo.FileName = SRCDSConfig.srcdsPath + "\\srcdsRD.exe";
            CSGOProcess.StartInfo.Arguments = SRCDSConfig.srcdsStartArguments;
            CSGOProcess.StartInfo.UseShellExecute = false;
            CSGOProcess.StartInfo.RedirectStandardError = true;
            CSGOProcess.StartInfo.RedirectStandardOutput = true;
            CSGOProcess.StartInfo.RedirectStandardInput = true;

            CSGOProcess.Start();
            CSGOProcess.BeginErrorReadLine();
            CSGOProcess.BeginOutputReadLine();

            //await ConnectRCon();
        }

        public async Task<string> StopSRCDS()
        {
            return await SendServerCommand("exit");
            //Process[] workers = Process.GetProcessesByName("srcds");
            //foreach (Process worker in workers) 
            //{
            //    //worker.StandardInput.WriteLine("exit");
                



            //    worker.Kill();
            //    worker.WaitForExit();
            //    worker.Dispose();
            //}
        }

        public async Task UpdateSRCDS()
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
            await Context.Channel.SendMessageAsync(UpdateText);
            await StartSRCDS();
            
        }



        void Update_Exited(object sender, EventArgs e)
        {
            
            Console.WriteLine("The srcds process has been exited/killed");
        }

        void Update_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                UpdateText += $"{e.Data}\n";
            }
            catch(Exception ex) { Console.WriteLine($"Error while recieving Error-Data: {ex.Message}"); }
            
        }

        void Update_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                UpdateText += $"{e.Data}\n";
                //var SentMessage = Context.Channel.SendMessageAsync(e.Data);
                //var Message = Message
            }
            catch (Exception ex) { Console.WriteLine($"Error while recieving Error-Data: {ex.Message}"); }
        }

        void steamcmd_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                Console.WriteLine($"Output Data Recieved: {e.Data}");
                //Context.RespondAsync(e.Data);
                if (e.Data.Contains("update"))
                {
                    ServerNeedsUpdate = true;
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error while recieving Error-Data: {ex.Message}"); }
        }

        public async Task ConnectRCon()
        {
            Console.WriteLine($"Trying to connect...");
           
            try
            {
                RconConnection = new RCON(IPAddress.Parse(SRCDSConfig.ServerIP), SRCDSConfig.ServerPort, SRCDSConfig.RconPassword, 2000);
                await RconConnection.ConnectAsync();
                Status status = await RconConnection.SendCommandAsync<Status>("status");
                Console.WriteLine($"Connected to: {status.Hostname}");
            }
            catch 
            {
                Console.WriteLine("Connection to SRCDS could not be established!");
            }
        }

        public async Task<string> SendServerCommand(string command)
        {
            //await ConnectRCon();
            return await RconConnection.SendCommandAsync(command);
        }

        public async Task<Status> GetServerStatus()
        {
            Status status = await RconConnection.SendCommandAsync<Status>("status");
            return status;
        }


    }
}