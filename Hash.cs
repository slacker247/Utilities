﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Utilities
{
    public class Hash
    {
        protected static String bytes2String(byte[] data)
        {
            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static string GetSha512(String input)
        {
            var hash = "";
            var data = Encoding.UTF8.GetBytes(input);
            using (SHA512 shaM = new SHA512Managed())
            {
                hash = bytes2String(shaM.ComputeHash(data));
            }
            return hash;
        }

        public static string GetMd5Hash(string input, MD5 md5Hash = null)
        {
            if (md5Hash == null)
                md5Hash = MD5.Create();

            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Return the hexadecimal string. 
            return bytes2String(data);
        }

        // Verify a hash against a string. 
        public static bool VerifyMd5Hash(string input, string hash, MD5 md5Hash = null)
        {
            if (md5Hash == null)
                md5Hash = MD5.Create();
            bool status = false;
            // Hash the input. 
            string hashOfInput = GetMd5Hash(input, md5Hash);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                status = true;
            }

            return status;
        }
    }
}
