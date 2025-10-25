using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ImageUtility.Models
{
    public class UserStatsHistory
    {
        [JsonPropertyName("SchemaVersion")]
        public int SchemaVersion { get; set; }
        [JsonPropertyName("Days")]
        public List<Day>? Days { get; set; }
    }

}
