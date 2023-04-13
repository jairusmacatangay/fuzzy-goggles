using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace VitasysEHR.Utility
{
    public class AesOperation
    {        
        private static Aes CreateCipher()
        {
            Aes cipher = Aes.Create();
            cipher.Mode = CipherMode.CBC;
            cipher.Padding = PaddingMode.ISO10126;
            cipher.Key = Encoding.UTF8.GetBytes("X/3mRrc8CpuI96/06BxqeERbB2dpl/jA");
            return cipher;
        }

        public static string? EncryptString(string? plainText, bool isUrl = false)
        {
            if (String.IsNullOrWhiteSpace(plainText))
                return null;

            Aes cipher = CreateCipher();
            cipher.GenerateIV();
            var iv = cipher.IV;
            ICryptoTransform encryptor = cipher.CreateEncryptor(cipher.Key, iv);
            
            var data = Encoding.UTF8.GetBytes(plainText);

            using (var cipherStream = new MemoryStream())
            {
                using (var tCryptoStream = new CryptoStream(cipherStream, encryptor, CryptoStreamMode.Write))
                using (var tBinaryWriter = new BinaryWriter(tCryptoStream))
                {
                    cipherStream.Write(iv);
                    tBinaryWriter.Write(data);
                    tCryptoStream.FlushFinalBlock();
                }
                
                if (isUrl == true)
                    return WebEncoders.Base64UrlEncode(cipherStream.ToArray());

                return Convert.ToBase64String(cipherStream.ToArray());
            }
        }

        public static string? DecryptString(string? cipherText, bool isUrl = false)
        {
            if (String.IsNullOrWhiteSpace(cipherText))
                return null;

            Aes cipher = CreateCipher();
            var iv = new byte[16];
            byte[] data;

            if (isUrl == true)
                data = WebEncoders.Base64UrlDecode(cipherText);
            else
                data = Convert.FromBase64String(cipherText);            

            Array.Copy(data, 0, iv, 0, iv.Length);

            ICryptoTransform decryptor = cipher.CreateDecryptor(cipher.Key, iv);

            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                using (var binaryWriter = new BinaryWriter(cs))
                {
                    binaryWriter.Write(data, iv.Length, data.Length - iv.Length);
                }

                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}
