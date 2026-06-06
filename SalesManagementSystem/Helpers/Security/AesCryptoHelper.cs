using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SalesManagementSystem.Helpers.Security
{
    public static class AesCryptoHelper
    {
        /// <summary>
        /// Băm khóa thành 256-bit bằng SHA-256 để đảm bảo khóa luôn đủ 32 bytes.
        /// </summary>
        private static byte[] GetHashKey(string key)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
            }
        }

        /// <summary>
        /// Mã hóa văn bản sử dụng AES-256 CBC.
        /// </summary>
        public static string Encrypt(string plainText, string fullKey, byte[] iv)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;
            if (iv == null || iv.Length != 16) throw new ArgumentException("IV must be 16 bytes.");

            byte[] keyBytes = GetHashKey(fullKey);

            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// Giải mã văn bản Base64 sử dụng AES-256 CBC.
        /// </summary>
        public static string Decrypt(string cipherTextBase64, string fullKey, byte[] iv)
        {
            if (string.IsNullOrEmpty(cipherTextBase64)) return cipherTextBase64;
            if (iv == null || iv.Length != 16) throw new ArgumentException("IV must be 16 bytes.");

            byte[] cipherText = Convert.FromBase64String(cipherTextBase64);
            byte[] keyBytes = GetHashKey(fullKey);

            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
