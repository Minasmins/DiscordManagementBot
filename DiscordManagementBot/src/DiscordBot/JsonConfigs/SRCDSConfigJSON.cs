using Newtonsoft.Json;

namespace DangerBotNamespace
{
    internal struct SRCDSConfigJSON
    {
        [JsonProperty("srcdsPath")]
        public string srcdsPath {get; private set; }

        [JsonProperty("srcdsStartArguments")]
        public string srcdsStartArguments {get; private set; }

        [JsonProperty("SteamCMDPath")]
        public string SteamCMDPath { get; private set; }
        
        [JsonProperty("SteamCMDArguments")]
        public string SteamCMDArguments { get; private set; }

        [JsonProperty("ServerIP")]
        public string ServerIP { get; private set; }

        [JsonProperty("ServerPort")]
        public ushort ServerPort { get; private set; }

        [JsonProperty("RconPassword")]
        public string RconPassword { get; private set; }
    }
}