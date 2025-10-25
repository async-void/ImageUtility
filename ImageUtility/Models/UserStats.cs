using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ImageUtility.Models
{
    public class UserStats
    {
        [JsonPropertyName("Converter")]
        public ConverterStats? Converter { get; set; }
        [JsonPropertyName("Renamer")]
        public RenamerStats? Renamer { get; set; }
        [JsonPropertyName("Resizer")]
        public ResizerStats? Resizer { get; set; }
    }
}
