using System.IO;
using System.Security.Cryptography;

namespace NotifiAlert
{
    public static class Crypto {
        public static readonly byte[] DefaultKey = { 41, 228, 124, 122, 59, 74, 78, 48, 29, 74, 44, 167, 127, 101, 6, 191 };
        public static readonly byte[] DefaultIV = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        
        public static byte[] EncryptBytes(byte[] plaintext) {
            return EncryptBytes(plaintext, DefaultKey, DefaultIV);
        }

        public static byte[] EncryptBytes(byte[] plaintext, byte[] key) {
            return EncryptBytes(plaintext, key, DefaultIV);
        }

        public static byte[] EncryptBytes(byte[] plaintext, byte[] key, byte[] iv)
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
        
        public static byte[] DecryptBytes(byte[] plaintext) {
            return DecryptBytes(plaintext, DefaultKey, DefaultIV);
        }

        public static byte[] DecryptBytes(byte[] plaintext, byte[] key) {
            return DecryptBytes(plaintext, key, DefaultIV);
        }

        public static byte[] DecryptBytes(byte[] cyphertext, byte[] key, byte[] iv)
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
    }
}