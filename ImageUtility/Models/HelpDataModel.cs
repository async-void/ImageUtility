using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ImageUtility.Models
{
    public class HelpDataModel
    {
        [JsonPropertyName("section")]
        public required string Section { get; set; }

        [JsonPropertyName("content")]
        public required string Content { get; set; }

        [JsonPropertyName("prefix")]
        public string? Prefix { get; set; }
        }
}
