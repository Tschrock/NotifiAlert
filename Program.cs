using System;
using System.Linq;
using Microsoft.Extensions.Logging;

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

            Client client = new Client(logger);

            client.Connect();

            WifiInfo[] networks = client.GetWirelessNetworks();
            
            WifiInfo[] orderedNetworks = networks.OrderByDescending(x => x.SignalPercent).ToArray();

            Console.WriteLine(orderedNetworks.ToStringTable(
                new [] { "SSID", "Signal Strength", "Secured" },
                n => n.Name,
                n => $"{n.SignalPercent}%",
                n => n.IsSecured ? "Yes" : "No"
            ));
        }
    }
}