using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace NotifiAlert
{
    class Program
    {
        public static void Warn(string value)
        {
            Console.WriteLine("Warn: " + value);
        }

        public static void Info(string value)
        {
            Console.WriteLine("Info: " + value);
        }

        public static void Debug(string value)
        {
            //Console.WriteLine("Debug: " + value);
        }

        public static void Debug2(string value)
        {
            //Console.WriteLine("Debug2: " + value);
        }

        public static string Hex(byte[] value)
        {
            return BitConverter.ToString(value).Replace("-", " ");
        }

        public static byte[] AES128ZeroKey = { 41, 228, 124, 122, 59, 74, 78, 48, 29, 74, 44, 167, 127, 101, 6, 191 };
        public static byte[] AES128ZeroIV = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static String serverHost = "192.168.100.1";
        private static int serverPort = 12345;
        private static int timeout = 12000;

        /// <summary>
        /// The device command.
        /// NOTE: The app source has the id for 'SendCommandType' and 'GetMacAddress' swapped when reading.
        /// </summary>
        public enum Command
        {
            StartCommunication = 0,
            SendWifiSSID = 16,
            SendWifiPWD = 17,
            SendDeviceName = 18,
            SendCloudID = 19,
            SendGMT = 20,
            SendCommandType = 22,
            SendServer = 21,
            GetWifiConnectionStatus = 64,
            GetScannedWifiSSIDNum = 65,
            GetScannedWifiSSID = 66,
            EndCommunication = -1,
            NoCommand = -1,
            GetMacAddress = 67
        }

        public class CommandPacket
        {
            public Command Command { get; set; }
            public byte[] Data { get; set; }
            public byte Status { get; set; }
            public CommandPacket(Command command, byte[] data, byte status = 0)
            {
                this.Command = command;
                this.Data = data;
                this.Status = status;
            }
            public byte[] ToBytes()
            {
                int unpaddedSize = 1 + 1 + Data.Length + 1 + 2;
                int paddedSize = NextMultiple(unpaddedSize, 16);
                byte[] buffer = new byte[paddedSize];
                int writeIndex = 0;

                // Command (1 byte)
                buffer[writeIndex++] = (byte)Command;

                // Command data length (1 byte)
                buffer[writeIndex++] = Data == null ? (byte)0 : (byte)Data.Length;

                // Command data (variable)
                if (Data != null && Data.Length > 0)
                {
                    Array.Copy(Data, 0, buffer, writeIndex, Data.Length);
                    writeIndex += Data.Length;
                }

                // Status (1 byte)
                buffer[writeIndex++] = Status;

                // CRC (2 bytes)
                short crc = GetCRC(new ReadOnlySpan<byte>(buffer, 0, writeIndex));
                byte[] crcBytes = BitConverter.GetBytes(crc);
                Array.Copy(crcBytes, 0, buffer, writeIndex, crcBytes.Length);

                return buffer;
            }

            public static CommandPacket FromBytes(byte[] buffer)
            {
                Command command = (Command)buffer[0];
                int dataSize = buffer[1];
                byte[] data = new byte[dataSize];
                Array.Copy(buffer, 2, data, 0, dataSize);
                byte status = buffer[dataSize + 2];
                short expectedCRC = BitConverter.ToInt16(new[] { buffer[dataSize + 3], buffer[dataSize + 4] });
                short actualCRC = GetCRC(new ReadOnlySpan<byte>(buffer, 0, dataSize + 3));
                if (expectedCRC != actualCRC)
                {
                    Warn("CRCs don't match! Something might be broken!");
                }
                return new CommandPacket(command, data, status);
            }
        }

        static void Main(string[] args)
        {
            Debug("Opening TCP Connection");
            TcpClient client = new TcpClient(serverHost, serverPort);
            NetworkStream stream = client.GetStream();
            stream.ReadTimeout = timeout;
            CommandPacket startCommResult = SendCommand(stream, new CommandPacket(Command.StartCommunication, new byte[0]), AES128ZeroKey);
            byte[] aesKey = startCommResult.Data;
            Info("Session AES key: " + Hex(aesKey));
            CommandPacket wifiNumResult = SendCommand(stream, new CommandPacket(Command.GetScannedWifiSSIDNum, new byte[0]), aesKey);
            int ssidCount = wifiNumResult.Data[0];
            Info("SSID Count: " + ssidCount);
            for (int i = 0; i < ssidCount / 3; ++i)
            {
                CommandPacket wifiSSIDRequest = new CommandPacket(Command.GetScannedWifiSSID, new byte[] { (byte)(i * 3), (byte)((i + 1) * 3 - 1) });
                CommandPacket wifiSSIDResult = SendCommand(stream, wifiSSIDRequest, aesKey);
                byte[] ssidListBytes = wifiSSIDResult.Data;
                Debug2("ssidList Bytes: " + Hex(ssidListBytes));
                string ssidList = System.Text.Encoding.ASCII.GetString(new ReadOnlySpan<byte>(ssidListBytes, 0, ssidListBytes.Length - 1));
                Debug("ssidList: " + ssidList);
                string[] ssids = ssidList.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var ssidBase64 in ssids)
                {
                    Debug2("SSID base64: " + ssidBase64);
                    byte[] ssidBytes = System.Convert.FromBase64String(ssidBase64);
                    Debug2("SSID Bytes: " + Hex(ssidBytes));
                    string ssid = System.Text.Encoding.UTF8.GetString(ssidBytes);
                    Info("SSID: " + ssid);
                }
            }

        }

        static CommandPacket SendCommand(Stream stream, CommandPacket packet, byte[] aesKey)
        {
            WriteCommand(stream, packet, aesKey);
            return ReadPacket(stream, packet.Command, aesKey);
        }

        static void WriteCommand(Stream stream, CommandPacket packet, byte[] aesKey)
        {
            Debug("Sending Command");
            Debug(packet.Command.ToString());
            if(packet.Data.Length > 0) Debug(Hex(packet.Data));
            byte[] commandBuffer = packet.ToBytes();
            Debug("Command Bytes: " + Hex(commandBuffer));
            byte[] encryptedBuffer = EncryptBytes(commandBuffer, AES128ZeroIV, aesKey);
            Debug2("Encrypted Bytes: " + Hex(encryptedBuffer));
            string base64 = System.Convert.ToBase64String(encryptedBuffer);
            Debug2("base64: " + base64);
            byte[] base64Bytes = System.Text.Encoding.ASCII.GetBytes(base64);
            //Debug("base64 Bytes: " + Hex(base64Bytes));
            stream.Write(base64Bytes);
            stream.Flush();
        }

        static byte[] EncryptBytes(byte[] plaintext, byte[] iv, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                ICryptoTransform encryptor = aes.CreateEncryptor();
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plaintext);
                        return memoryStream.ToArray();
                    }
                }
            }
        }

        static byte[] DecryptBytes(byte[] cyphertext, byte[] iv, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                ICryptoTransform decryptor = aes.CreateDecryptor();
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(cyphertext);
                        return memoryStream.ToArray();
                    }
                }
            }
        }

        static int NextMultiple(int x, int n)
        {
            return ((x + n - 1) / n) * n;
        }


        public static short GetCRC(ReadOnlySpan<byte> var0)
        {
            int var1 = 65535;

            for (int var2 = 0; var2 < var0.Length; ++var2)
            {
                var1 ^= var0[var2] & 255;

                for (int var3 = 0; var3 < 8; ++var3)
                {
                    if ((var1 & 1) != 0)
                    {
                        var1 = var1 >> 1 ^ '\ua001';
                    }
                    else
                    {
                        var1 >>= 1;
                    }
                }
            }

            return (short)var1;
        }

        static CommandPacket ReadPacket(Stream stream, Command command, byte[] aesKey)
        {
            List<byte> buffer = new List<byte>();
            int ch;
            while (true)
            {
                ch = stream.ReadByte();
                if (ch == -1 || ch == '\n') break;
                else if (ch == 0) continue;
                else buffer.Add((byte)ch);
            }
            Debug("Reading Response");
            if (buffer.Count >= 10)
            {
                //Debug("base64 Bytes: " + Hex(buffer.ToArray()));
                string base64 = System.Text.Encoding.ASCII.GetString(buffer.ToArray());
                Debug2("base64: " + base64);
                byte[] encryptedBuffer = System.Convert.FromBase64String(base64);
                Debug2("Encrypted Bytes: " + Hex(encryptedBuffer));
                byte[] decryptedBuffer = DecryptBytes(encryptedBuffer, AES128ZeroIV, aesKey);
                Debug("Decrypted Bytes: " + Hex(decryptedBuffer));
                CommandPacket packet = CommandPacket.FromBytes(decryptedBuffer);
                if (packet.Command == command) return packet;
            }
            return null;
        }
    }
}
