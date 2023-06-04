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
    }
}