
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using static System.Text.Encoding;

using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using System.Net.NetworkInformation;

namespace NotifiAlert
{
    public class Client
    {
        public readonly string serverHost = "192.168.100.1";
        public readonly int serverPort = 12345;
        public readonly int timeout = 12000;

        private TcpClient client;
        private NetworkStream stream;
        private byte[] sessionEncryptionKey = Crypto.DefaultKey;
        private ILogger log;

        public Client(ILogger logger)
        {
            log = logger;
        }

        /// <summary>
        /// Connects to the device.
        /// </summary>
        public void Connect()
        {
            if (client == null)
            {
                // Connect
                log.LogDebug("Connecting to {0}:{1} with timeout {2}", serverHost, serverPort, timeout);
                client = new TcpClient(serverHost, serverPort);
                stream = client.GetStream();
                stream.ReadTimeout = timeout;

                // Get the AES key for this session
                sessionEncryptionKey = Net_StartCommunication();
                log.LogTrace("Session AES key: {0}", sessionEncryptionKey.Hex());
            }
        }

        public WifiInfo[] GetWirelessNetworks()
        {
            log.LogDebug("Retrieving wireless networks");

            // Make sure we're connected
            Connect();

            // Get the number of SSIDs
            int ssidCount = Net_GetScannedWifiSSIDNum();
            log.LogDebug("SSID Count: {0}", ssidCount);

            // Retrieve the SSIDs in groups of 3.
            // You can probably request them in any amount you want, but the
            // app uses 3 so that's what I'll do too.
            List<WifiInfo> wifiInfo = new List<WifiInfo>(ssidCount);
            for (int i = 0; i < ssidCount; i += 3)
            {
                int endIndex = Math.Min(i + 2, ssidCount - 1);
                WifiInfo[] group = Net_GetScannedWifiSSID(i, endIndex);
                wifiInfo.AddRange(group);
            }
            return wifiInfo.ToArray();
        }

        public void GetWifi(string email, string[] macAddresses)
        {

        }

        internal byte[] Net_StartCommunication()
        {
            CommandPacket result = SendCommand(new CommandPacket(Command.StartCommunication));
            return result.Data;
        }

        internal byte Net_SendWifiSSID(string ssid)
        {
            CommandPacket result = SendCommand(new CommandPacket(Command.SendWifiSSID, UTF8.GetBytes(ssid)));
            return result.Status;
        }

        internal byte Net_SendWifiPWD(string password)
        {
            CommandPacket result = SendCommand(new CommandPacket(Command.SendWifiPWD, UTF8.GetBytes(password)));
            return result.Status;
        }

        internal byte Net_SendDeviceName(string name)
        {
            CommandPacket result = SendCommand(new CommandPacket(Command.SendDeviceName, UTF8.GetBytes(name)));
            return result.Status;
        }

        internal byte Net_SendCloudID(string email)
        {
            CommandPacket result = SendCommand(new CommandPacket(Command.SendCloudID, ASCII.GetBytes(email)));
            return result.Status;
        }

        internal byte Net_SendGMT(byte[] data)
        {
            CommandPacket result = SendCommand(new CommandPacket(Command.SendGMT, data));
            return result.Status;
        }

        internal byte Net_SendCommandType(bool unknown)
        {
            CommandPacket result = SendCommand(new CommandPacket(Command.SendCommandType, unknown ? (byte)1 : (byte)0));
            return result.Status;
        }

        internal byte Net_SendServer(bool useDevelopmentServer)
        {
            CommandPacket result = SendCommand(new CommandPacket(Command.SendServer, useDevelopmentServer ? (byte)1 : (byte)0));
            return result.Status;
        }

        internal byte[] Net_GetWifiConnectionStatus()
        {
            CommandPacket result = SendCommand(new CommandPacket(Command.GetWifiConnectionStatus));
            return result.Data;
        }

        internal int Net_GetScannedWifiSSIDNum()
        {
            CommandPacket result = SendCommand(new CommandPacket(Command.GetScannedWifiSSIDNum));
            return result.Data[0];
        }

        internal WifiInfo[] Net_GetScannedWifiSSID(int startIndex, int endIndex)
        {
            // Send the command
            CommandPacket result = SendCommand(new CommandPacket(Command.GetScannedWifiSSID, (byte)startIndex, (byte)endIndex));

            // Get the result data
            byte[] ssidListBytes = result.Data;
            log.LogTrace("SSID List Bytes: {0}", ssidListBytes.Hex());

            // The result data is a cstring, so trim off the trailing null
            ReadOnlySpan<byte> trimmedListBytes = ssidListBytes.TrimBytes(0);

            // Convert the bytes to a string so we can split it easier
            string ssidList = ASCII.GetString(trimmedListBytes);

            // Split apart the ssids
            string[] ssids = ssidList.Split(';', StringSplitOptions.RemoveEmptyEntries);

            // Get the data for each ssid
            WifiInfo[] wifiInfo = new WifiInfo[ssids.Length];
            for (int i = 0; i < ssids.Length; i++)
            {
                // Base 64 decode the wifi data
                byte[] ssidDataBytes = System.Convert.FromBase64String(ssids[i]);
                log.LogTrace("SSID Data Bytes: {0}", ssidDataBytes.Hex());

                // Convert the bytes to a string
                string ssidDataString = UTF8.GetString(ssidDataBytes);
                log.LogTrace("SSID Data: {0}", ssidDataString);

                // Split apart the data
                string[] ssidDataParts = ssidDataString.Split(',', 3);

                // Sanity check
                if (ssidDataParts.Length != 3)
                {
                    log.LogWarning("Unknown SSID format. Expected 3 fields but got {0} ({1})", ssidDataParts.Length, ssidDataString);
                }

                // Add the network to the list
                wifiInfo[i] = new WifiInfo(
                    ssidDataParts.Length > 0 ? ssidDataParts[0] : "Unknown",
                    ssidDataParts.Length > 1 ? int.Parse(ssidDataParts[1]) : 0,
                    ssidDataParts.Length > 2 && int.Parse(ssidDataParts[2]) == 1
                );
            }

            // Return the wifi info
            return wifiInfo;
        }

        internal byte Net_EndCommunication()
        {
            CommandPacket result = SendCommand(new CommandPacket(Command.EndCommunication));
            return result.Status;
        }

        internal PhysicalAddress Net_GetMacAddress()
        {
            CommandPacket result = SendCommand(new CommandPacket(Command.GetMacAddress));
            return new PhysicalAddress(result.Data);
        }

        /// <summary>
        /// Sends a command packet and reads the response.
        /// </summary>
        /// <param name="packet">The command packet.</param>
        /// <param name="aesKey">The AES key to encrypt the packet with.</param>
        /// <returns></returns>
        protected CommandPacket SendCommand(CommandPacket packet)
        {
            WriteCommand(packet);
            return ReadPacket(packet.Command);
        }

        /// <summary>
        /// Writes a command packet to the underlying stream.
        /// </summary>
        /// <param name="packet">The command packet.</param>
        protected void WriteCommand(CommandPacket packet)
        {
            log.LogDebug("Sending Command [{0}]", packet.Command);

            // Get the command bytes
            byte[] commandBuffer = packet.ToBytes();
            log.LogDebug("Command Bytes: " + Util.Hex(commandBuffer));

            // Encrypt the packet
            byte[] encryptedBuffer = Crypto.EncryptBytes(commandBuffer, sessionEncryptionKey);
            log.LogTrace($"Encrypted Bytes: {Util.Hex(encryptedBuffer)}");

            // Base 64 encode
            string base64 = System.Convert.ToBase64String(encryptedBuffer);

            // Get Bytes
            byte[] base64Bytes = ASCII.GetBytes(base64);

            // Write the packet
            stream.Write(base64Bytes);
            stream.Flush();
        }

        /// <summary>
        /// Reads from the stream until one of the specified characters is found.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
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
        /// <returns>The CommandPacket, or `null` if it could not be read.</returns>
        protected CommandPacket ReadPacket(Command command)
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
                string base64 = ASCII.GetString(trimmedBuffer).Trim();

                // Base 64 decode
                byte[] encryptedBuffer = System.Convert.FromBase64String(base64);
                log.LogTrace("Encrypted Bytes: {0}", encryptedBuffer.Hex());

                // Decrypt
                byte[] decryptedBuffer = Crypto.DecryptBytes(encryptedBuffer, sessionEncryptionKey);
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