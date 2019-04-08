using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Management;

namespace Utilities.system
{
    public class SystemResources
    {
        public static string GetLocalIPAddress()
        {
            String ip = "UNK";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var _ip in host.AddressList)
            {
                if (_ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip = _ip.ToString();
                }
            }
            return ip;
        }

        //int totalHits = 0;
        static PerformanceCounter cpuCounter = new PerformanceCounter();
        static bool m_Init = false;
        protected static void init()
        {
            if (!m_Init)
            {
                cpuCounter.CategoryName = "Processor";
                cpuCounter.CounterName = "% Processor Time";
                cpuCounter.InstanceName = "_Total";
                m_Init = true;
            }
        }

        /// <summary>
        /// Usage percentage
        /// </summary>
        /// <returns></returns>
        public static float getCPUCounter()
        {
            init();

            // now matches task manager reading
            float secondValue = cpuCounter.NextValue();

			return secondValue;
//			return 1;
		}

        public static int getCPUCount()
        {
            int count = Environment.ProcessorCount;
            return count;
        }

        public static long getAvailableRAM()
        {
            long memSize = 0;

            GetPhysicallyInstalledSystemMemory(out memSize);
            return memSize;
        }

        public static String getHostName()
        {
            return System.Net.Dns.GetHostEntry("").HostName;
        }

        public static String getOSName()
        {
            string result = string.Empty;
            System.OperatingSystem osInfo = System.Environment.OSVersion;
            result = osInfo.VersionString;
            return result;
        }

        public static long getOSDriveTotalSize()
        {
            long totalSize = 0;
            var systemDriveName = Environment.GetEnvironmentVariable("SystemDrive");
            var systemDrive = new DriveInfo(systemDriveName);
            totalSize = systemDrive.TotalSize;
            return totalSize;
        }

        public static long getOSDriveFreeSpace()
        {
            long totalSize = 0;
            var systemDriveName = Environment.GetEnvironmentVariable("SystemDrive");
            var systemDrive = new DriveInfo(systemDriveName);
            totalSize = systemDrive.TotalFreeSpace;
            return totalSize;
        }

        //private String pollUsage()
        //{
        //    int cpuPercent = (int)getCPUCounter();
        //    if (cpuPercent >= 90)
        //    {
        //        totalHits = totalHits + 1;
        //        if (totalHits == 60) // seconds
        //        {
        //            // TODO : cpu usage has been high for 60 seconds
        //        }
        //        totalHits = 0;
        //    }
        //    else
        //    {
        //        totalHits = 0;
        //    }

        //    String cpuUsage = cpuPercent + " % CPU";
        //    String ramFree = getAvailableRAM() + " RAM Free";
        //    String usageOver = totalHits + " seconds over 20% usage";
        //    return cpuUsage + "\n" + ramFree + "\n" + usageOver;
        //}

        public static bool is64BitProcess()
        {
            return IntPtr.Size == 8;
        }

        public static bool is64BitOperatingSystem()
        {
            bool status = false;
            // Clearly if this is a 64-bit process we must be on a 64-bit OS.
            if (is64BitProcess())
                status = true;
            else
            {
                // Ok, so we are a 32-bit process, but is the OS 64-bit?
                // If we are running under Wow64 than the OS is 64-bit.
                bool isWow64;

                status = ModuleContainsFunction("kernel32.dll", "IsWow64Process") &&
                        IsWow64Process(GetCurrentProcess(), out isWow64) &&
                        isWow64;
            }
            return status;
        }

        static bool ModuleContainsFunction(string moduleName, string methodName)
        {
            bool status = false;
            IntPtr hModule = GetModuleHandle(moduleName);
            if (hModule != IntPtr.Zero)
                status = GetProcAddress(hModule, methodName) != IntPtr.Zero;
            return status;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        extern static bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool isWow64);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern static IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        extern static IntPtr GetModuleHandle(string moduleName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        extern static IntPtr GetProcAddress(IntPtr hModule, string methodName);
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);
    }
}
