using System;
using System.Text.Json.Serialization;

namespace NotifiAlert.Cloud.Models
{
    public partial class Device
    {
        [JsonPropertyName("DeviceId")]
        public long DeviceId { get; set; }

        [JsonPropertyName("DeviceProfile")]
        public string DeviceProfile { get; set; }

        [JsonPropertyName("DeviceType")]
        public string DeviceType { get; set; }

        [JsonPropertyName("ServiceType")]
        public string ServiceType { get; set; }

        [JsonPropertyName("SSID")]
        public string Ssid { get; set; }

        [JsonPropertyName("TimeZone")]
        public long TimeZone { get; set; }

        [JsonPropertyName("TimeOffset")]
        public long TimeOffset { get; set; }

        [JsonPropertyName("FwUpAutoFlag")]
        public bool FwUpAutoFlag { get; set; }

        [JsonPropertyName("FirmwareUpgrade")]
        public bool FirmwareUpgrade { get; set; }

        [JsonPropertyName("StatusRenewal")]
        public double StatusRenewal { get; set; }

        [JsonPropertyName("DeviceStatus")]
        public DeviceStatus DeviceStatus { get; set; }

        [JsonPropertyName("LastIotEventTime")]
        public string LastIotEventTime { get; set; }

        [JsonPropertyName("LastLiveFeedTime")]
        public object LastLiveFeedTime { get; set; }

        [JsonPropertyName("DeviceDisplayName")]
        public string DeviceDisplayName { get; set; }

        [JsonPropertyName("DeviceDisplayIcon")]
        public Uri DeviceDisplayIcon { get; set; }

        [JsonPropertyName("RelationshipType")]
        public string RelationshipType { get; set; }

        [JsonPropertyName("SipSearchKey")]
        public string SipSearchKey { get; set; }

        [JsonPropertyName("SipUsername")]
        public string SipUsername { get; set; }

        [JsonPropertyName("DeviceIconChecksum")]
        public string DeviceIconChecksum { get; set; }

        [JsonPropertyName("IsDefaultIcon")]
        public bool IsDefaultIcon { get; set; }

        [JsonPropertyName("TuyaIndicator")]
        public long TuyaIndicator { get; set; }

        [JsonPropertyName("NotifiCutOff")]
        public string NotifiCutOff { get; set; }

        [JsonPropertyName("Accessories")]
        public object[] Accessories { get; set; }
    }
}