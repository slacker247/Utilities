using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;

namespace Utilities.system
{
    public class Execute
    {
        public static bool g_CreateNoWindow = true;

        public delegate void ReceivedDataCallback(String data);
        public static event ReceivedDataCallback receivedDataEvent;

        private static void receivedData(object sendingProcess, DataReceivedEventArgs outLine)
        {
            System.Console.WriteLine("Data received: " + outLine.Data);

            if (receivedDataEvent != null && !String.IsNullOrEmpty(outLine.Data))
            {
                receivedDataEvent(outLine.Data);
            }
        }

        public delegate void ExitedCallback(int exitCode);
        public static event ExitedCallback exitedEvent;

        private static void exited(object sendingProcess, EventArgs args)
        {
            Process proc = (Process)sendingProcess;
            if (proc != null && proc.HasExited)
            {
                Console.WriteLine(proc.StandardOutput.ReadToEnd());
                Console.WriteLine("Exit time:    {0}\r\n" +
                    "Exit code:    {1}\r\nElapsed time: {2}", proc.ExitTime, proc.ExitCode, 3325);
                if (exitedEvent != null)
                {
                    exitedEvent(proc.ExitCode);
                }
            }
        }

        private static String g_UserName = "";
        private static String g_Password = "";
        private static String g_Domain = "";
        public static void setCreds(String username, String password, String domain)
        {
            g_UserName = username;
            g_Password = password;
            g_Domain = domain;
        }
        
        public static int runCmd(String cmd, String args = "", String workingDir = "")
        {
            int status = -1;
            {
                // Execute Convert.bat on dir.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo(cmd);
                //-really-quiet
                if(!String.IsNullOrEmpty(workingDir))
                    procStartInfo.WorkingDirectory = workingDir;
                procStartInfo.Arguments = args;
                procStartInfo.UseShellExecute = true;

                if (!String.IsNullOrEmpty(g_UserName) &&
                    !String.IsNullOrEmpty(g_Password) &&
                    !String.IsNullOrEmpty(g_Domain))
                {
                    procStartInfo.UseShellExecute = false;
                    procStartInfo.Verb = "runas";

                    procStartInfo.UserName = g_UserName;
                    System.Security.SecureString pass = new System.Security.SecureString();
                    for (int i = 0; i < g_Password.Length; i++)
                        pass.AppendChar(g_Password[i]);
                    procStartInfo.Password = pass;
                    procStartInfo.Domain = g_Domain;
                    procStartInfo.LoadUserProfile = true;
                }
                
                if(receivedDataEvent != null || 
                    exitedEvent != null)
                {
                    // The following commands are needed to redirect the standard output.
                    // This means that it will be redirected to the Process.StandardOutput StreamReader.
                    procStartInfo.UseShellExecute = false;
                    procStartInfo.RedirectStandardInput = true;
                    procStartInfo.RedirectStandardOutput = true;
                    procStartInfo.RedirectStandardError = true;
                }
                // Do not create the black window.
                procStartInfo.CreateNoWindow = g_CreateNoWindow;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process g_Proc = new System.Diagnostics.Process();
                g_Proc.StartInfo = procStartInfo;

                if (receivedDataEvent != null ||
                    exitedEvent != null)
                {
                    g_Proc.EnableRaisingEvents = true;
                    g_Proc.OutputDataReceived += new DataReceivedEventHandler(receivedData);
                    g_Proc.ErrorDataReceived += new DataReceivedEventHandler(receivedData);
                    g_Proc.Exited += new EventHandler(exited);
                }
                string result = "Command Failed";
                try
                {
                    System.Console.WriteLine("Starting command.");
                    if (g_Proc.Start())
                        result = "Executing Command...";
                    while (!g_Proc.HasExited)
                        Thread.Sleep(120);
                    result = "Finished.";
                }
                catch (Win32Exception e)
                {
                    result = "Error:" + e.Message;
                    //g_Stop = true;
                }
                catch (Exception ex)
                {
                    result = "Error:" + ex.Message;
                    //g_Stop = true;
                }
                try
                {
                    if (g_Proc.HasExited)
                    {
                        status = g_Proc.ExitCode;
                    }
                }
                catch (Exception ex)
                {
                }
                System.Console.WriteLine("Result: " + result);
                //System.Console.ReadKey();
            }
            return status;
        }
    }
}
