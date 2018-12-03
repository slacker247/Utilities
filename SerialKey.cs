using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class SerialKey
    {
        public static byte[,] m_ByteSeeds = {
#region Keys
#if KEY00
                                 {94, 89, 233},
#endif
#if KEY01
                                 {162, 248, 172},
#endif
#if KEY02
                                 {68, 198, 226},
#endif
#if KEY03
                                 {23, 44, 160},
#endif
#if KEY04
                                 {121, 198, 23},
#endif
#if KEY05
                                 {141, 6, 250},
#endif
#endregion
                                 {0, 0, 0}
                             }; // 0 - 255
#if INTERNAL

        public static string getSerialKey(String unique)
        {
            /// windows key - GM9FP-TQBBV-TV3RW-9873P-9G84B
            // first 8 chars are seed
            // last 4 chars are checksum?
            // chars are numbers between 48 - 57 = 9
            // and letters are between 65 - 90 = 25
            String result = "";
            Int64 seed = 0;
            String strSeed = Hash.GetMd5Hash(unique);
            System.Console.WriteLine(strSeed);
            String firstSeed = strSeed.Substring(0, 8);
            seed = System.Convert.ToInt64(firstSeed, 16);
            result = PKV_MakeKey(seed);
            return result;
        }

        public static string PKV_MakeKey(Int64 seed)
        {
            const int numBytes = 6; // m_ByteSeeds.Length;
            byte[] keyBytes = new byte[numBytes];
            int i;
            String result;

            // Fill KeyBytes with values derived from Seed.
            // The parameters used here must be extactly the same
            // as the ones used in the PKV_CheckKey function.
            // A real key system should use more than four bytes.
            for (i = 0; i < numBytes; i++)
            {
                keyBytes[i] = PKV_GetKeyByte(seed, 
                    m_ByteSeeds[i,0], 
                    m_ByteSeeds[i,1], 
                    m_ByteSeeds[i,2]);
            }

            // the key string begins with a hexidecimal string of the seed
            String strSeed = seed.ToString("X8");
            result = strSeed.Substring(strSeed.Length - 8);

            // then is followed by hexidecimal strings of each byte in the key
            for (i = 0; i < numBytes; i++)
                result = result + keyBytes[i].ToString("X2");

            // add checksum to key string
            String checkSumHex = PKV_GetChecksum(result).ToString("X4");
            result = result + checkSumHex;

            // Add some hyphens to make it easier to type
            i = result.Length - numBytes;
            while (i > 1)
            {
                result = result.Insert(i, "-");
                i = i - numBytes;
            }
            return result;
        }
#endif
        public static byte PKV_GetKeyByte(Int64 seed, byte a, byte b, byte c)
        {
            byte result = 0;
            a = (byte)(a % 25);
            b = (byte)(b % 3);
            if (a % 2 == 0)
                result = (byte)(((seed << a) & 0x000000FF) ^ ((seed << b) | c));
            else
                result = (byte)(((seed << a) & 0x000000FF) ^ ((seed << b) & c));
            return result;
        }

        /// <summary>
        /// Creates a check sum given a string that looks
        /// like: TV3RW9873P
        /// then returns a value like: 9G84B as an int
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int PKV_GetChecksum(String s)
        {
            int left, right, sum;
            int i, result;

            left = 0x0056;
            right = 0x00AF;

            if (s.Length > 0)
            {
                for (i = 1; i < s.Length; i++)
                {
                    right = (byte)(right + (int)s[i]);
                    if (right > 0x00FF)
                    {
                        right = (byte)(0x00FF - right);
                    }
                    left = (byte)(right + left);
                    if (left > 0x00FF)
                    {
                        left = (byte)(0x00FF - left);
                    }
                }
            }

            result = (left << 8) + right;
            return result;
        }
    }
}
