﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Collections;
using System.Threading.Tasks;
using System.Reflection;
using System.Drawing;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;

namespace Utilities
{
    public class FileIO
    {
        protected static Utilities.threading.ThreadManager Mgmr = new threading.ThreadManager();
        protected static List<String> m_Files = new List<String>();

        public static string CnvrtUnit(long source)
        {
            const int byteConversion = 1024;
            double bytes = Convert.ToDouble(source);

            if (bytes >= Math.Pow(byteConversion, 3)) //GB Range
            {
                return string.Concat(Math.Round(bytes / Math.Pow(byteConversion, 3), 2), " GB");
            }
            else if (bytes >= Math.Pow(byteConversion, 2)) //MB Range
            {
                return string.Concat(Math.Round(bytes / Math.Pow(byteConversion, 2), 2), " MB");
            }
            else if (bytes >= byteConversion) //KB Range
            {
                return string.Concat(Math.Round(bytes / byteConversion, 2), " KB");
            }
            else //Bytes
            {
                return string.Concat(bytes, " Bytes");
            }
        }

        public static int copyStream(Stream input, String outFileName, bool overwrite = true)
        {
            if (File.Exists(outFileName) && overwrite)
                File.Delete(outFileName);
            Stream output = File.Create(outFileName);
            // Insert null checking here for production
            byte[] buffer = new byte[8192];

            int bytesRead;
            int bytesWrote = 0;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
                bytesWrote += bytesRead;
            }
            output.Flush();
            return bytesWrote;
        }

        public static int scrubFile(ref String file)
        {
            int status = -1;
            if (file != null)
            {
                file = Regex.Replace(file, "\\\\", "/", RegexOptions.IgnoreCase);
                file = Regex.Replace(file, "//192.168.0.11/media", "/Media/", RegexOptions.IgnoreCase);
                file = Regex.Replace(file, "//filedbsrv/media", "/Media/", RegexOptions.IgnoreCase);
                file = Regex.Replace(file, "y:/", "/Media/", RegexOptions.IgnoreCase);
                file = Regex.Replace(file, "//filedbsrv.j-a-m.net/media", "/Media/", RegexOptions.IgnoreCase);
                status = 0;
            }
            return status;
        }

        public static bool checkExt(String ext)
        {
            bool status = false;
            switch (ext)
            {
                case ".mov":
                    status = true;
                    break;
                case ".m4v":
                    status = true;
                    break;
                case ".mp4":
                    status = true;
                    break;
            }
            return status;
        }

        public static String fileHash(String file)
        {
            String hashed = "";
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (SHA1Managed sha1 = new SHA1Managed())
                    {
                        StringBuilder formatted = null;
                        byte[] hash = sha1.ComputeHash(bs);
                        formatted = new StringBuilder(2 * hash.Length);
                        foreach (byte b in hash)
                        {
                            formatted.AppendFormat("{0:X2}", b);
                        }
                        hashed = formatted.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return hashed;
        }

        public static String humanReadableSize(long size)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            int s = 0;
            double len = size;

            while (len >= 1024 && s < suffixes.Length - 1)
            {
                s++;
                len = len / 1024;
            }

            string result = String.Format("{0:0.##} {1}", len, suffixes[s]);
            return result;
        }

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("Kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetDiskFreeSpaceEx
        (
            string lpszPath,                    // Must name a folder, must end with '\'.
            ref long lpFreeBytesAvailable,
            ref long lpTotalNumberOfBytes,
            ref long lpTotalNumberOfFreeBytes
        );
        
        // Does not work with unc paths
        public static long getTotalFreeSpace(string driveName)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    return drive.TotalFreeSpace;
                }
            }
            return -1;
        }

        public delegate void fileFoundCallback(String file);
        public static event fileFoundCallback fileFound;

        public static List<String> getFiles(String dir)
        {
            Mgmr.MaxThreads = 16;
            Mgmr.start();
            getFiles(dir, "");
            Mgmr.waitAll();
            Mgmr.stop();
            return m_Files;
        }

        // filter splits multiple file extensions based on the '|'
        // character.
        // Returns an array of strings.
        public static List<String> getFiles(String dir, String filter)
        {
            m_Files.Clear();
            //DirSearch(dir, filter);
            String[] subFilter = filter.Split('|');
            for (int n = 0; n < subFilter.Length; n++)
            {
                //foreach (String f in Directory.EnumerateFiles(dir, subFilter[n], SearchOption.AllDirectories))
                //{
                //    m_Files.Add(f);
                //}
                var t_dir = dir;
                var t_filter = subFilter[n];
                Mgmr.addThread(() => DirSearch(t_dir, t_filter));
            }
            return m_Files;
        }

        protected static void DirSearch(String sDir, String filter)
        {
            try
            {
                String[] files = null;
                if (filter.Length > 0)
                    files = Directory.GetFiles(sDir, filter);
                else
                    files = Directory.GetFiles(sDir);
                // this is probably pointless as parallel with the lock in it.
                Parallel.ForEach(files, new ParallelOptions() { MaxDegreeOfParallelism = 20 }, (f) =>
                {
                    lock (m_Files)
                    {
                        m_Files.Add(f);
                    }
                    if (fileFound != null)
                        Mgmr.addThread(() => fileFound(f));
                });
                foreach (String d in Directory.GetDirectories(sDir))
                {
                    var t_dir = d;
                    var t_filter = filter;
                    Mgmr.addThread(() => DirSearch(t_dir, t_filter));
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        public static bool byteListToFile(String fileName, List<byte[]> bytes)
        {
            bool retVal = false;
            bool first = false;
            foreach (byte[] byteArray in bytes)
            {
                retVal = byteArrayToFile(fileName, byteArray, first);
                if (!retVal)
                    break;
                if (!first)
                    first = true;
            }
            return retVal;
        }

        public static bool byteArrayToFile(string fileName, byte[] byteArray, bool append = false)
        {
            try
            {
                System.IO.FileMode fm = FileMode.Create;
                if (append)
                    fm = FileMode.Append;
                // Open file for reading
                System.IO.FileStream fileStream =
                   new System.IO.FileStream(fileName, fm,
                                            System.IO.FileAccess.Write);
                // Writes a block of bytes to this stream using data from
                // a byte array.
                fileStream.Write(byteArray, 0, byteArray.Length);

                // close file stream
                fileStream.Close();

                return true;
            }
            catch (Exception ex)
            {
                // Error
                Console.WriteLine("Exception caught in process: {0}",
                                  ex.ToString());
            }

            // error occured, return false
            return false;
        }

        public static String replaceInvalidFileChars(String name)
        {
            string illegal = new String(name.ToCharArray());
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            foreach (char c in invalid)
            {
                illegal = illegal.Replace(c.ToString(), "");
            }
            return illegal;
        }
    }
}
