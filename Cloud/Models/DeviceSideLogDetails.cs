using System.Text.Json.Serialization;

namespace NotifiAlert.Cloud.Models
{
    public class DeviceSideLogDetails
    {
        [JsonPropertyName("AcPower")]
        public bool AcPower { get; set; }

        [JsonPropertyName("BatteryStatus")]
        public int BatteryStatus { get; set; }

        [JsonPropertyName("Thermal")]
        public int Thermal { get; set; }

        [JsonPropertyName("WiFiStrength")]
        public int WiFiStrength { get; set; }

        [JsonPropertyName("DeviceFlashBroken")]
        public bool DeviceFlashBroken { get; set; }
    }
}
