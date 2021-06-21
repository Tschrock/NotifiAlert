using System.Text.Json.Serialization;

namespace NotifiAlert.Cloud.Models
{
    public class DeviceSideLogResponse
    {
        [JsonPropertyName("Type")]
        public int Type { get; set; }

        [JsonPropertyName("Result")]
        public bool Result { get; set; }

        [JsonPropertyName("Service")]
        public string Service { get; set; }

        [JsonPropertyName("PremiumTrial")]
        public bool PremiumTrial { get; set; }

        [JsonPropertyName("Offset")]
        public int Offset { get; set; }
    }
}

/*
{"Type":1,"Result":true,"Service":"Standard","PremiumTrial":false,"Offset":60}
*/
