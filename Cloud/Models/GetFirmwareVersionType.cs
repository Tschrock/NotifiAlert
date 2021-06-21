
using System.Text.Json.Serialization;

namespace NotifiAlert.Cloud.Models
{
    public enum GetFirmwareVersionType
    {
        [JsonPropertyName("video")] 
        VIDEO,
        
        [JsonPropertyName("alert")]
        ALERT
    }
}
