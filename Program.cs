using System;
using System.Linq;

using Microsoft.Extensions.Logging;

using NotifiAlert.Doorbell;

namespace NotifiAlert
{
    class Program
    {
        static void Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("NotifiAlert", LogLevel.Warning)
                    .AddConsole()
                    .AddDebug();
            });

            ILogger logger = loggerFactory.CreateLogger("NotifiAlert");

            DoorbellClient client = new DoorbellClient(logger);

            client.Connect();

            WifiInfo[] networks = client.GetWirelessNetworks();

            WifiInfo[] orderedNetworks = networks.OrderByDescending(x => x.SignalPercent).ToArray();

            Console.WriteLine(orderedNetworks.ToStringTable(
                new[] { "SSID", "Signal Strength", "Secured" },
                n => n.Name,
                n => $"{n.SignalPercent}%",
                n => n.IsSecured ? "Yes" : "No"
            ));

            var result = client.Net_GetWifiConnectionStatus();
            Console.WriteLine(result.Data.Hex());
            Console.WriteLine(new byte[] { result.Status }.Hex());

            var mac = client.Net_GetMacAddress();
            Console.WriteLine(mac.GetAddressBytes().Hex());

            client.Net_SendGMT(GetTimeZone());
            client.Net_SendServer(false);
            client.Net_SendWifiPWD("");
            client.Net_SendDeviceName("Side Door");
            client.Net_SendCloudID("x@gmail.com");
            client.Net_SendCommandType(false);
            client.Net_SendWifiSSID("wifi");
            client.Net_EndCommunication();
       }

        private static byte[] GetTimeZone()
        {
            TimeZoneInfo timeZone = TimeZoneInfo.Local;
            String id = timeZone.Id;
            try
            {
                TimeSpan rawOffset = timeZone.BaseUtcOffset;
                TimeSpan duration = rawOffset.Duration();
                byte[] bArr = {
                    rawOffset.Hours < 0 ? (byte)1 : (byte)0,
                    (byte)duration.Hours,
                    (byte)duration.Minutes
                };
                return Util.ConcatBytes(
                    System.Text.Encoding.UTF8.GetBytes(id),
                    bArr
                );
            }
            catch
            {
                return new byte[] { 0, 0 };
            }
        }

    }
}
