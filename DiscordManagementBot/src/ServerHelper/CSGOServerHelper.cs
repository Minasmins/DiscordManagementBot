
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;   
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Bson;

namespace DangerBotNamespace
{
    class CSGOGameServer
    {
        private SRCDSConfigJSON OtherConfig {get; set;}
        public Process CSGOProcess { get; set; }
        public Boolean ServerNeedsUpdate { get; private set; }
        static CSGOGameServer? Instance;

        public static CSGOGameServer ServerInstance
        {
            get{
                if (Instance == null)
                {
                    Instance = new CSGOGameServer();
                }
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
            OtherConfig = JsonConvert.DeserializeObject<SRCDSConfigJSON>(Json);
            ServerNeedsUpdate = false;
        }

        public async Task StartSRCDS()
        {
            System.Diagnostics.Process CSGOProcess = new System.Diagnostics.Process();
            CSGOProcess.EnableRaisingEvents = true;
            CSGOProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_OutputDataReceived);
            CSGOProcess.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_ErrorDataReceived);
            CSGOProcess.Exited += new System.EventHandler(process_Exited);

            CSGOProcess.StartInfo.FileName = OtherConfig.srcdsPath + "\\srcds";
            CSGOProcess.StartInfo.Arguments = OtherConfig.srcdsStartArguments;
            CSGOProcess.StartInfo.UseShellExecute = false;
            CSGOProcess.StartInfo.RedirectStandardError = true;
            CSGOProcess.StartInfo.RedirectStandardOutput = true;
            CSGOProcess.StartInfo.RedirectStandardInput = true;

            CSGOProcess.Start();
            CSGOProcess.BeginErrorReadLine();
            CSGOProcess.BeginOutputReadLine();
        }

        public async Task StopSRCDS()
        {
            StreamWriter myStreamWriter = CSGOProcess.StandardInput;
            myStreamWriter.WriteLine("exit");
            //CSGOProcess.Kill();
        }
            
        void process_Exited(object sender, EventArgs e)
        {
            Console.WriteLine(string.Format("process exited with code {0}\n", CSGOProcess.ExitCode.ToString()));
        }

        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"Error Data Recieved: {e.Data}");
            if (e.Data.Contains("update"))
            {
                ServerNeedsUpdate = true;
            }
        }

        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"Output Data Recieved: {e.Data}");
            if (e.Data.Contains("update"))
            {
                ServerNeedsUpdate = true;
            }
        }

        public void Exitprocess()
        {
            CSGOProcess.Kill();
        }

        public async Task UpdateCSGO()
        {
            System.Diagnostics.Process SteamCMDProcess = new System.Diagnostics.Process();
            SteamCMDProcess.EnableRaisingEvents = true;
            SteamCMDProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_OutputDataReceived);
            SteamCMDProcess.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_ErrorDataReceived);
            SteamCMDProcess.Exited += new System.EventHandler(process_Exited);

            SteamCMDProcess.StartInfo.FileName = OtherConfig.SteamCMDPath + "\\steamcmd";
            //SteamCMDProcess.StartInfo.Arguments = OtherConfig.srcdsStartArguments;
            SteamCMDProcess.StartInfo.UseShellExecute = false;
            SteamCMDProcess.StartInfo.RedirectStandardError = true;
            SteamCMDProcess.StartInfo.RedirectStandardOutput = true;
            SteamCMDProcess.StartInfo.RedirectStandardInput = true;

            SteamCMDProcess.Start();
            SteamCMDProcess.BeginErrorReadLine();
            SteamCMDProcess.BeginOutputReadLine();

            StreamWriter myStreamWriter = SteamCMDProcess.StandardInput;

            myStreamWriter.WriteLine("LIST DISK");
            myStreamWriter.Close();

            
        }



        public static void OnStartCSGO()
        {
            Console.WriteLine("Event erfolgreich weggefeiert");
        }
    }
}