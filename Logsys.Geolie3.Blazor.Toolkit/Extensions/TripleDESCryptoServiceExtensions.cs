using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ERP.DEMO.Toolkit.Extensions
{
    public static class TripleDESCryptoServiceExtensions
    {
        private static byte[] InitializationVector = Encoding.UTF8.GetBytes("Q5ZvRB7K"); // 8 bits
        private static byte[] Key = new byte[24]; // 24 bits
        private static TripleDESCryptoServiceProvider encryptor = new TripleDESCryptoServiceProvider();

        /// <summary>
        /// Retourne une chaine cryptée à partir d'une chaine non cryptée.
        /// </summary>
        /// <param name="stringToEncrypt">La chaine à crypter</param>
        /// <param name="key">La clef de cryptage sur 24 caractères</param>
        /// <returns>La chaine cryptée.</returns>
        public static string Encrypt(this string stringToEncrypt, string key = "TAYtHldS2m8a84Q7g6wNfT9D")
        {
            Key = Encoding.UTF8.GetBytes(key); // 24 bits
            ICryptoTransform transform = encryptor.CreateEncryptor(Key, InitializationVector);
            byte[] data = Encoding.UTF8.GetBytes(stringToEncrypt);
            return Convert.ToBase64String(CryptoStreamFlush(transform, data));
        }

        /// <summary>
        /// Retourne une chaine décryptée à partir d'une chaine cryptée.
        /// </summary>
        /// <param name="stringToDecrypt">La chaine à décrypter.</param>
        /// <param name="key">La clef de cryptage sur 24 caractères</param>
        /// <returns>La chaine décryptée.</returns>
        public static string Decrypt(this string stringToDecrypt, string key = "TAYtHldS2m8a84Q7g6wNfT9D")
        {
            Key = Encoding.UTF8.GetBytes(key); // 24 bits
            ICryptoTransform transform = encryptor.CreateDecryptor(Key, InitializationVector);
            byte[] data = Convert.FromBase64String(stringToDecrypt);
            return Encoding.UTF8.GetString(CryptoStreamFlush(transform, data));
        }

        /// <summary>
        /// Retourne le flux à crypter ou décrypter
        /// </summary>
        /// <param name="transform">La transformation à oppérer.</param>
        /// <param name="data">Les données à traiter.</param>
        /// <returns>Le flux de données traitées.</returns>
        private static byte[] CryptoStreamFlush(ICryptoTransform transform, byte[] data)
        {
            MemoryStream stream = new MemoryStream();
            try
            {
                CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Write);
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();
                cryptoStream.Close();
                return stream.ToArray();
            }
            finally
            {
                if (stream != null)
                    stream.Dispose();
            }
        }
    }
}
