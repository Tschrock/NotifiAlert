
using System.Text.Json.Serialization;

namespace NotifiAlert.Cloud.Models
{
    public partial class Welcome
    {
        [JsonPropertyName("Account")]
        public string Account { get; set; }

        [JsonPropertyName("Message")]
        public string Message { get; set; }
    }
}
