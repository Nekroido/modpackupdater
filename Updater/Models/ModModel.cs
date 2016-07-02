using Newtonsoft.Json;
using System;
using Updater.Classes;

namespace Updater.Models
{
    public class ModModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }

        [JsonProperty("size")]
        public int FileSize { get; set; }

        [JsonProperty("updated")]
        private int updated { get; set; }
        public DateTime Updated
        {
            get { return UnixTimeHelper.DateTimeFromUnixTimestampSeconds(updated); }
        }

        [JsonProperty("checksum")]
        public string Checksum { get; set; }
    }
}
