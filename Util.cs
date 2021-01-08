using System;
using System.Linq;

namespace NotifiAlert
{
    public static class Util
    {
        public static string Hex(this byte[] value)
        {
            return BitConverter.ToString(value).Replace("-", " ");
        }

        public static string Hex(this ReadOnlySpan<byte> value)
        {
            return BitConverter.ToString(value.ToArray()).Replace("-", " ");
        }
        
        public static ReadOnlySpan<byte> Trim(this byte[] buffer, params byte[] bytesToTrim)
        {
            int start = 0;
            while(start < buffer.Length && bytesToTrim.Contains(buffer[start]))
            {
                ++start;
            }

            int end = buffer.Length - 1;
            while(end >= 0 && bytesToTrim.Contains(buffer[end]))
            {
                --end;
            }

            if(start > end) return new byte[0];
            return new ReadOnlySpan<byte>(buffer, start, end - start + 1);
        }
        
        public static byte[] ConcatBytes(params byte[][] buffers)
        {
            int len = buffers.Sum(b => b.Length);
            byte[] newBuffer = new byte[len];
            for(int bufferIndex = 0, writeIndex = 0; bufferIndex < buffers.Length; ++bufferIndex) {
                byte[] buffer = buffers[bufferIndex];
                Array.Copy(buffer, 0, newBuffer, writeIndex, buffer.Length);
                writeIndex += buffer.Length;
            }
            return newBuffer;
        }
    }
}