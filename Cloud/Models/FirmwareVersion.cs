
using System.Text.Json.Serialization;

namespace NotifiAlert.Cloud.Models
{
    public class FirmwareVersion
    {
        [JsonPropertyName("SerialNo")]
        public int SerialNo { get; set; }

        [JsonPropertyName("DeviceType")]
        public int DeviceType { get; set; }

        [JsonPropertyName("FileTitle")]
        public int FileTitle { get; set; }

        [JsonPropertyName("CurrentVer")]
        public int CurrentVer { get; set; }

        [JsonPropertyName("MD5")]
        public int MD5 { get; set; }

        [JsonPropertyName("URL")]
        public int URL { get; set; }
    }
}
