using System.Text.Json.Serialization;

namespace NotifiAlert.Cloud.Models
{
    public class LoginToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public string ExpiresIn { get; set; }

        [JsonPropertyName("account")]
        public string Account { get; set; }

        [JsonPropertyName("acc_id")]
        public string AccountId { get; set; }

        [JsonPropertyName("register")]
        public string Register { get; set; }

        [JsonPropertyName("proxy")]
        public string Proxy { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName(".issued")]
        public string IssuedAt { get; set; }

        [JsonPropertyName(".expires")]
        public string ExpiresAt { get; set; }
    }
}