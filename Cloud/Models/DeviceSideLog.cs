using System.Text.Json.Serialization;

namespace NotifiAlert.Cloud.Models
{
    public class DeviceSideLog
    {
        [JsonPropertyName("TypeNum")]
        public int TypeNum { get; set; }

        [JsonPropertyName("DeviceID")]
        public string DeviceID { get; set; }

        [JsonPropertyName("DeviceType")]
        public string DeviceType { get; set; }

        [JsonPropertyName("Version")]
        public string Version { get; set; }

        [JsonPropertyName("CreateDate")]
        public string CreateDate { get; set; }

        [JsonPropertyName("CreateTime")]
        public string CreateTime { get; set; }

        [JsonPropertyName("StatusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("StatusDescription")]
        public string StatusDescription { get; set; }

        [JsonPropertyName("Details")]
        public DeviceSideLogDetails Details { get; set; }
    }
}

/*
{
	"TypeNum":	1,
	"DeviceID":	"237dfb80219d0d597dddd9e1cbbe4e5e",
	"DeviceType":	"NotifiAlert",
	"Version":	"3.3.8",
	"CreateDate":	"2021.06.21",
	"CreateTime":	"00:55:41",
	"StatusCode":	0,
	"StatusDescription":	"Success",
	"Details":	{
		"AcPower":	true,
		"BatteryStatus":	1000,
		"Thermal":	0,
		"WiFiStrength":	72,
		"DeviceFlashBroken":	false
	}
}
*/
