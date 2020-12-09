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
        
        public static ReadOnlySpan<byte> TrimBytes(this byte[] buffer, params byte[] bytesToTrim)
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
    }
}