using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using NotifiAlert.Cloud.Models;
using NotifiAlert.Cloud.Util;

namespace NotifiAlert.Cloud
{
    public class CloudClient
    {
        public static readonly Uri PRODUCTION_API_ROOT = new Uri("https://notifi-backend.azurewebsites.net/");
        public static readonly Uri DEVELOPMENT_API_ROOT = new Uri("https://notifi-backend-dev.azurewebsites.net/");
        private HttpClient client;
        private Uri rootApi;
        private LoginToken loginToken;

        public CloudClient() : this(PRODUCTION_API_ROOT, new HttpClient()) { }
        public CloudClient(string apiRoot) : this(new Uri(apiRoot), new HttpClient()) { }
        public CloudClient(Uri apiRoot) : this(apiRoot, new HttpClient()) { }
        public CloudClient(string apiRoot, HttpClient client) : this(new Uri(apiRoot), client) { }
        public CloudClient(HttpClient client) : this(PRODUCTION_API_ROOT, client) { }
        public CloudClient(Uri apiRoot, HttpClient client)
        {
            client.BaseAddress = apiRoot;
        }

        public async Task<LoginToken> Login(string username, string password, bool rememberMe)
        {
            HttpResponseMessage response = await client.PostAsync(
                "api/AppUser/Login",
                new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("Password", password),
                    new KeyValuePair<string, string>("RememberMe", rememberMe.ToString()),
                })
            );
            response.EnsureSuccessStatusCode();
            string responseData = await response.Content.ReadAsStringAsync();
            loginToken = JsonSerializer.Deserialize<LoginToken>(responseData);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginToken.AccessToken);
            return loginToken;
        }

        public async Task<string> GetAppSetting()
        {
            HttpResponseMessage response = await client.GetAsync("api/NotifiApp/GetAppSetting");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        
        public Task<Device[]> FindDevices() => GetAsync<Device[]>("api/Relationship/FindDevices");
        public Task<Welcome> GetWelcomeMessage() => GetAsync<Welcome>("api/AppUser/Welcome");
        public Task<string> GetCurrentEmail() => GetAsync<string>("api/AppUser/CurrentEmail");
        public Task<FirmwareVersions> GetFirmwareVersionInfo(GetFirmwareVersionType type) => GetAsync<FirmwareVersions>("api/firmware/VersionInfo?type=" + type);
        public Task<EventLog[]> GetEventLog(string deviceId) => GetAsync<EventLog[]>("api/event/QueryEvent?DeviceKey=" + deviceId);
        public Task<DeviceSideLogResponse> PostDeviceLogging(DeviceSideLog log) => PostAsync<DeviceSideLog, DeviceSideLogResponse>("api/logging/DeviceSide", log);

        private Task<T> GetAsync<T>(string url, Dictionary<string, dynamic> queryParams)
        {
            return GetAsync<T, Dictionary<string, dynamic>>(url, queryParams);
        }

        private Task<T> GetAsync<T, U>(string url, U queryObject) where U : class
        {
            return GetAsync<T>(url + QueryStringFormatter<U>.Serialize(queryObject));
        }

        private async Task<T> GetAsync<T>(string url)
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseData = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseData);
        }
        private async Task<U> PostAsync<T, U>(string url, T content)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(url, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<U>();
        }

    }
}
