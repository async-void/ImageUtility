using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ImageUtility.Models
{
    public class ConverterStats
    {
        [JsonPropertyName("Total")]
        public int Total { get; set; }

        [JsonPropertyName("Success")]
        public int Success { get; set; }

        [JsonPropertyName("Fail")]
        public int Fail { get; set; }
    }
}
