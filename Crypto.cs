using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Utilities
{
    // Code based on the book "C# 3.0 in a nutshell by Joseph Albahari" (pages 630-632)
    // and from this StackOverflow post by somebody called Brett
    // http://stackoverflow.com/questions/202011/encrypt-decrypt-string-in-net/2791259#2791259
    public class Crypto
    {
        private static byte[] m_Salt = Encoding.ASCII.GetBytes("Ent3r your oWn S@lt v@lu# h#r3");

        public static void setSalt(String salt)
        {
            m_Salt = Encoding.ASCII.GetBytes(salt);
        }

        public static string Encrypt(String textToEncrypt, String encryptionPassword)
        {
            RijndaelManaged algorithm = GetAlgorithm(encryptionPassword);

            byte[] encryptedBytes;
            using (ICryptoTransform encryptor = algorithm.CreateEncryptor(algorithm.Key, algorithm.IV))
            {
                byte[] bytesToEncrypt = Encoding.UTF8.GetBytes(textToEncrypt);
                encryptedBytes = InMemoryCrypt(bytesToEncrypt, encryptor);
            }
            String encryptStr = "";
            if (encryptedBytes != null)
                encryptStr = Convert.ToBase64String(encryptedBytes);
            return encryptStr;
        }

        public static string Decrypt(String encryptedText, String encryptionPassword)
        {
            RijndaelManaged algorithm = GetAlgorithm(encryptionPassword);

            byte[] descryptedBytes;
            using (ICryptoTransform decryptor = algorithm.CreateDecryptor(algorithm.Key, algorithm.IV))
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                descryptedBytes = InMemoryCrypt(encryptedBytes, decryptor);
            }
            String decryptStr = "";
            if (descryptedBytes != null)
                decryptStr = Encoding.UTF8.GetString(descryptedBytes);
            return decryptStr;
        }

        // Performs an in-memory encrypt/decrypt transformation on a byte array.
        private static byte[] InMemoryCrypt(byte[] data, ICryptoTransform transform)
        {
            byte[] bytes = null;
            MemoryStream memory = new MemoryStream();
            if (data.Length > 0)
            {
                try
                {
                    using (Stream stream = new CryptoStream(memory, transform, CryptoStreamMode.Write))
                    {
                        stream.Write(data, 0, data.Length);
                    }
                    bytes = memory.ToArray();
                }
                catch (CryptographicException cex)
                {
                }
                catch (Exception ex)
                {
#if DEBUG
                        Console.WriteLine("Error with encryption.");
#endif
                }
            }
            return bytes;
        }

        // Defines a RijndaelManaged algorithm and sets its key and Initialization Vector (IV) 
        // values based on the encryptionPassword received.
        private static RijndaelManaged GetAlgorithm(string encryptionPassword)
        {
			// Rfc2898DeriveBytes has a default interation of 1000
            // Create an encryption key from the encryptionPassword and salt.
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(encryptionPassword, m_Salt);

            // Declare that we are going to use the Rijndael algorithm with the key that we've just got.
            RijndaelManaged algorithm = new RijndaelManaged();
            int bytesForKey = algorithm.KeySize / 8;
            int bytesForIV = algorithm.BlockSize / 8;
            algorithm.Key = key.GetBytes(bytesForKey);
            algorithm.IV = key.GetBytes(bytesForIV);
            return algorithm;
        }
    }
}
