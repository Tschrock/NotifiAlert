
using System.Text.Json.Serialization;

namespace NotifiAlert.Cloud.Models
{
    public class FirmwareVersions
    {
        [JsonPropertyName("ItemCount")]
        public int ItemCount { get; set; }

        [JsonPropertyName("Items")]
        public FirmwareVersion Items { get; set; }
    }
}
