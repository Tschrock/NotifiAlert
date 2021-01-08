using System;
using System.Text.Json.Serialization;

namespace NotifiAlert.Cloud.Models
{
    public partial class DeviceStatus
    {
        [JsonPropertyName("AcPowerFlag")]
        public bool AcPowerFlag { get; set; }

        [JsonPropertyName("BatteryRemaining")]
        public long BatteryRemaining { get; set; }

        [JsonPropertyName("Temperature")]
        public long Temperature { get; set; }

        [JsonPropertyName("WiFiStrength")]
        public long WiFiStrength { get; set; }

        [JsonPropertyName("IpAddress")]
        public string IpAddress { get; set; }

        [JsonPropertyName("IotMessageFlag")]
        public bool IotMessageFlag { get; set; }

        [JsonPropertyName("MdzSettingFlag")]
        public bool MdzSettingFlag { get; set; }

        [JsonPropertyName("ResolutionFlag")]
        public bool ResolutionFlag { get; set; }

        [JsonPropertyName("NotifiFwVersion")]
        public string NotifiFwVersion { get; set; }

        [JsonPropertyName("McuFwVersion")]
        public string McuFwVersion { get; set; }
    }
}