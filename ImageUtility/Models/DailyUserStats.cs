using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ImageUtility.Models
{
    public class DailyUserStats
    {
        [JsonPropertyName("Date")]
        public DateTimeOffset Date { get; set; }
        [JsonPropertyName("Stats")]
        public UserStats? Stats { get; set; }

    }
}
