using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Updater.Classes;

namespace Updater.Models
{
    public class VersionModel
    {
        [JsonProperty("updated")]
        private int updated { get; set; }
        public DateTime Updated
        {
            get { return UnixTimeHelper.DateTimeFromUnixTimestampSeconds(updated); }
        }

        [JsonProperty("mods")]
        public List<ModModel> Mods { get; set; }
    }
}
