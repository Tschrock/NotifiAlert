using System;

namespace NotifiAlert
{
    /// <summary>
    /// A command packet.
    /// </summary>
    public class CommandPacket
    {
        /// <summary>
        /// The Command.
        /// </summary>
        /// <value>The Command.</value>
        public Command Command { get; set; }

        /// <summary>
        /// The data for the command.
        /// </summary>
        /// <value>An array of bytes containing the command data.</value>
        public byte[] Data { get; set; }

        /// <summary>
        /// The status.
        /// </summary>
        /// <value>A byte representing the status.</value>
        public byte Status { get; set; }

        /// <summary>
        /// Creates a new CommandPacket.
        /// </summary>
        /// <param name="command">The command.</param>
        public CommandPacket(Command command) : this(command, new byte[0]) { }
        
        /// <summary>
        /// Creates a new CommandPacket.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="data">The command data.</param>
        public CommandPacket(Command command, byte[] data) : this(command, data, 0) { }

        /// <summary>
        /// Creates a new CommandPacket.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="data">The command data.</param>
        /// <param name="status">The command status.</param>
        public CommandPacket(Command command, byte[] data, byte status)
        {
            this.Command = command;
            this.Data = data;
            this.Status = status;
        }

        /// <summary>
        /// Writes the CommandPacket to a buffer.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            // Calculate the final buffer size
            int unpaddedSize = 1 + 1 + Data.Length + 1 + 2;

            // Pad to the next multiple of 16
            int paddedSize = NextMultiple(unpaddedSize, 16);

            // Create the write buffer
            byte[] buffer = new byte[paddedSize];
            int writeIndex = 0;

            // Write the command (1 byte)
            buffer[writeIndex++] = (byte)Command;

            // Write the data length (1 byte)
            buffer[writeIndex++] = Data == null ? (byte)0 : (byte)Data.Length;

            // Write the data (variable)
            if (Data != null && Data.Length > 0)
            {
                Array.Copy(Data, 0, buffer, writeIndex, Data.Length);
                writeIndex += Data.Length;
            }

            // Write the status (1 byte)
            buffer[writeIndex++] = Status;

            // Calculate the CRC
            short crc = GetCRC(new ReadOnlySpan<byte>(buffer, 0, writeIndex));

            // Write the CRC (2 bytes)
            byte[] crcBytes = BitConverter.GetBytes(crc);
            Array.Copy(crcBytes, 0, buffer, writeIndex, crcBytes.Length);

            // Return the buffer
            return buffer;
        }

        /// <summary>
        /// Read the CommandPacket from a buffer. Throws a PacketException if there is an error reading the packet.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static CommandPacket FromBytes(byte[] buffer)
        {
            // Check the min message size
            if (buffer.Length < 5)
            {
                throw new PacketException("Invalid Packet Size. Packet must be at least 5 bytes long.");
            }

            // Read the command
            Command command = (Command)buffer[0];

            // Read the data size
            int dataSize = buffer[1];

            // Check that the data size stays within the buffer.
            if (dataSize + 5 > buffer.Length)
            {
                throw new PacketException($"Invalid Data Field Size. The packet claims to have a data size of {dataSize} bytes, but it's only large enough for {buffer.Length - 5} bytes of data.");
            }

            // Read the data
            byte[] data = new byte[dataSize];
            Array.Copy(buffer, 2, data, 0, dataSize);

            // Read the status
            byte status = buffer[dataSize + 2];

            // Read the expected CRC
            short expectedCRC = BitConverter.ToInt16(new[] { buffer[dataSize + 3], buffer[dataSize + 4] });

            // Calculate the actual CRC
            short actualCRC = GetCRC(new ReadOnlySpan<byte>(buffer, 0, dataSize + 3));

            // Check that the CRCs match
            if (expectedCRC != actualCRC)
            {
                throw new PacketException("CRCs don't match! Something might be broken!");
            }

            // Build a new packet
            return new CommandPacket(command, data, status);
        }

        private static int NextMultiple(int x, int n)
        {
            return ((x + n - 1) / n) * n;
        }

        private static short GetCRC(ReadOnlySpan<byte> var0)
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
    }
}
