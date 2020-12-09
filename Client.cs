
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;
using System.Linq;
using System;

namespace NotifiAlert
{
    public class Client
    {
        public readonly string serverHost = "192.168.100.1";
        public readonly int serverPort = 12345;
        public readonly int timeout = 12000;

        private TcpClient client;

        private NetworkStream stream;
        private ILogger log;

        public Client(ILogger logger)
        {
            log = logger;
        }

        public WifiInfo[] GetWirelessNetworks()
        {
            log.LogDebug("Retrieving wireless networks");

            List<WifiInfo> wifiInfo = new List<WifiInfo>();

            // Make sure we're connected
            Connect();

            // Start communication
            CommandPacket startCommResult = SendCommand(new CommandPacket(Command.StartCommunication), Crypto.DefaultKey);

            // Get the new AES key
            byte[] aesKey = startCommResult.Data;
            log.LogTrace("Session AES key: {0}", aesKey.Hex());

            // Get the number of SSIDs
            CommandPacket wifiNumResult = SendCommand(new CommandPacket(Command.GetScannedWifiSSIDNum), aesKey);
            int ssidCount = wifiNumResult.Data[0];
            log.LogDebug("SSID Count: {0}", ssidCount);

            // Retrieve the SSIDs in groups of 3.
            // You can probably request them in any amount you want, but the
            // app uses 3 so that's what I'll do too.
            for (int i = 0; i < ssidCount; i += 3)
            {
                // Build the request
                CommandPacket wifiSSIDRequest = new CommandPacket(
                    Command.GetScannedWifiSSID,
                    new byte[] {
                        (byte)i,
                        (byte)Math.Min(ssidCount - 1, i + 2)
                    }
                );

                // Send the command
                CommandPacket wifiSSIDResult = SendCommand(wifiSSIDRequest, aesKey);

                // Get the result data
                byte[] ssidListBytes = wifiSSIDResult.Data;
                log.LogTrace("SSID List Bytes: {0}", ssidListBytes.Hex());

                // The result data is a cstring, so trim off the trailing null
                ReadOnlySpan<byte> trimmedListBytes = ssidListBytes.TrimBytes(0);

                // Convert the bytes to a string so we can split it easier
                string ssidList = System.Text.Encoding.ASCII.GetString(trimmedListBytes);

                // Split apart the ssids
                string[] ssids = ssidList.Split(';', StringSplitOptions.RemoveEmptyEntries);

                // Get the data for each ssid
                foreach (var ssidDataBase64 in ssids)
                {
                    // Base 64 decode the wifi data
                    byte[] ssidDataBytes = System.Convert.FromBase64String(ssidDataBase64);

                    // Convert the bytes to a string
                    string ssidDataString = System.Text.Encoding.UTF8.GetString(ssidDataBytes);
                    log.LogTrace("SSID Data: {0}", ssidDataString);
                    
                    // Split apart the data
                    string[] ssidDataParts = ssidDataString.Split(',', 3);

                    // Sanity check
                    if (ssidDataParts.Length != 3)
                    {
                        log.LogWarning("Unknown SSID format. Expected 3 fields but got {0} ({1})", ssidDataParts.Length, ssidDataString);
                    }

                    // Add the network to the list
                    wifiInfo.Add(new WifiInfo(
                        ssidDataParts.Length > 0 ? ssidDataParts[0] : "Unknown",
                        ssidDataParts.Length > 1 ? int.Parse(ssidDataParts[1]) : 0,
                        ssidDataParts.Length > 2 && int.Parse(ssidDataParts[2]) == 1
                    ));
                }
            }

            return wifiInfo.ToArray();
        }

        /// <summary>
        /// Connects to the device.
        /// </summary>
        public void Connect()
        {
            if (client == null)
            {
                log.LogDebug("Connecting to {0}:{1} with timeout {2}", serverHost, serverPort, timeout);
                client = new TcpClient(serverHost, serverPort);
                stream = client.GetStream();
                stream.ReadTimeout = timeout;

            }
        }

        /// <summary>
        /// Sends a command packet and reads the response.
        /// </summary>
        /// <param name="packet">The command packet.</param>
        /// <param name="aesKey">The AES key to encrypt the packet with.</param>
        /// <returns></returns>
        protected CommandPacket SendCommand(CommandPacket packet, byte[] aesKey)
        {
            WriteCommand(packet, aesKey);
            return ReadPacket(packet.Command, aesKey);
        }

        /// <summary>
        /// Writes a command packet to the underlying stream.
        /// </summary>
        /// <param name="packet">The command packet.</param>
        /// <param name="aesKey">The AES key to encrypt the packet with.</param>
        protected void WriteCommand(CommandPacket packet, byte[] aesKey)
        {
            log.LogDebug("Sending Command [{0}]", packet.Command);

            // Get the command bytes
            byte[] commandBuffer = packet.ToBytes();
            log.LogDebug("Command Bytes: " + Util.Hex(commandBuffer));

            // Encrypt the packet
            byte[] encryptedBuffer = Crypto.EncryptBytes(commandBuffer, aesKey);
            log.LogTrace($"AES Key: {Util.Hex(aesKey)}");
            log.LogTrace($"Encrypted Bytes: {Util.Hex(encryptedBuffer)}");

            // Base 64 encode
            string base64 = System.Convert.ToBase64String(encryptedBuffer);

            // Get Bytes
            byte[] base64Bytes = System.Text.Encoding.ASCII.GetBytes(base64);

            // Write the packet
            stream.Write(base64Bytes);
            stream.Flush();
        }

        protected byte[] ReadUntil(params byte[] bytes)
        {
            bool started = false;
            List<byte> readBuffer = new List<byte>();
            while (true)
            {
                int ch = stream.ReadByte();
                if (ch == -1) break;

                var found = bytes.Contains((byte)ch);
                if (found)
                {
                    if (started) break;
                    else continue;
                }
                else
                {
                    if (!started) started = true;
                    readBuffer.Add((byte)ch);
                }
            }
            return readBuffer.ToArray();
        }

        /// <summary>
        /// Reads a command packet from the underlying stream.
        /// </summary>
        /// <param name="command">The expected command.</param>
        /// <param name="aesKey">The AES key to decrypt the packet with.</param>
        /// <returns>The CommandPacket, or `null` if it could not be read.</returns>
        protected CommandPacket ReadPacket(Command command, byte[] aesKey)
        {
            log.LogDebug("Reading Command");

            // Read the next line
            byte[] buffer = ReadUntil((byte)'\n');

            // Trim any extra null bytes/newlines
            ReadOnlySpan<byte> trimmedBuffer = buffer.TrimBytes(0, (byte)'\n', (byte)'\r');
            log.LogDebug("Read {0} bytes: {1}", trimmedBuffer.Length, trimmedBuffer.Hex());

            // Check buffer length
            if (trimmedBuffer.Length >= 10)
            {
                // Get buffer as string
                string base64 = System.Text.Encoding.ASCII.GetString(trimmedBuffer).Trim();

                // Base 64 decode
                byte[] encryptedBuffer = System.Convert.FromBase64String(base64);
                log.LogTrace("Encrypted Bytes: {0}", encryptedBuffer.Hex());

                // Decrypt
                byte[] decryptedBuffer = Crypto.DecryptBytes(encryptedBuffer, aesKey);
                log.LogTrace("AES Key: {0}", aesKey.Hex());
                log.LogTrace("Decrypted Bytes: {0}", decryptedBuffer.Hex());

                // Decode packet
                CommandPacket packet = CommandPacket.FromBytes(decryptedBuffer);

                // Make sure command matches
                if (packet.Command == command) return packet;
            }
            return null;
        }
    }
}