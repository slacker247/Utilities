using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Utilities.system
{
    public class Execute
    {
        protected int g_ExitCode = -1;
        public bool g_Async = false;
        public bool g_CreateNoWindow = true;
        protected System.Diagnostics.Process g_Proc = null;

        public delegate void ReceivedDataCallback(String data);
        public event ReceivedDataCallback receivedDataEvent;

        public int GetPid()
        {
            int id = -1;
            if (g_Proc != null)
                id = g_Proc.Id;
            return id;
        }

        private void receivedData(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (sendingProcess is Process)
                receivedData((Process)sendingProcess, outLine.Data);
        }

        protected void receivedData(Process proc, String data)
        {
            //System.Console.WriteLine("Data received: " + data);

            if (receivedDataEvent != null && !String.IsNullOrEmpty(data))
            {
                receivedDataEvent(data);
            }
        }

        public void writeData(String cmd, String args)
        {
            writeData(cmd + " " + args);
        }

        public void writeDataAsync(String cmd, String args)
        {
            writeDataAsync(cmd + " " + args);
        }

        public void writeDataAsync(String cmd)
        {
            if (g_Proc != null &&
                !g_Proc.HasExited)
            {
                g_Proc.StandardInput.WriteLine(cmd);
            }
        }

        public void writeData(String cmd)
        {
            if(g_Proc != null &&
                !g_Proc.HasExited)
            {
                TimeSpan timeout = new TimeSpan(0, 0, 15);
                g_Proc.StandardInput.WriteLine(cmd);

                char[] buf = new char[1024];
                String response = "";
                int bufLength = 1;
                bool foundCmd = false;
                DateTime start = DateTime.Now;
                while (!foundCmd &&
                    (DateTime.Now - start) < timeout)
                {
                    if (g_Proc != null)
                    {
                        if (bufLength > 0)
                        {
                            Thread th = new Thread(() =>
                                { bufLength = g_Proc.StandardOutput.Read(buf, 0, 1023); });
                            th.Start();
                            if (!th.Join(TimeSpan.FromSeconds(15)))
                            {
                                bufLength = 0;
                                th.Abort();
                            }

                            if (g_Proc != null)
                            {
                                g_Proc.StandardOutput.DiscardBufferedData();
                                response = new String(buf).TrimEnd('\0');
                                List<String> lines = new List<string>();
                                lines.AddRange(response.Split('\n'));
                                Array.Clear(buf, 0, 1024);
                                foreach (String line in lines)
                                {
                                    if (line.Contains(cmd))
                                        foundCmd = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        foundCmd = true;
                    }
                }
                start = DateTime.Now;
                while (g_Proc != null &&
                    bufLength > 0 &&
                    !(response.Trim().EndsWith(">")) &&
                    (DateTime.Now - start) < timeout
                )
                {
                    if (g_Proc != null &&
                        bufLength > 0)
                    {
                        Thread th = new Thread(() =>
                        { if(g_Proc != null)
                            bufLength = g_Proc.StandardOutput.Read(buf, 0, 1023); });
                        th.Start();
                        if (!th.Join(TimeSpan.FromSeconds(15)))
                        {
                            bufLength = 0;
                            th.Abort();
                        }

                        if (g_Proc != null)
                        {
                            g_Proc.StandardOutput.DiscardBufferedData();
                            response += new String(buf).TrimEnd('\0');
                            Array.Clear(buf, 0, 1024);
                        }
                    }
                    else
                        bufLength = 0;
                }
                receivedData(g_Proc, response);
                if(g_Proc != null)
                    g_Proc.StandardOutput.DiscardBufferedData();
            }
        }

        public delegate void ExitedCallback(int exitCode);
        public event ExitedCallback exitedEvent;

        private void exited(object sendingProcess, EventArgs args)
        {
            Process proc = (Process)sendingProcess;
            if (proc != null && proc.HasExited)
            {
                // This is for webcam, test webcam if changing.
                if(!g_Async)
                    receivedData(proc, proc.StandardOutput.ReadToEnd());
                Console.WriteLine("Exit time:    {0}\r\n" +
                    "Exit code:    {1}\r\nElapsed time: {2}", proc.ExitTime, proc.ExitCode, 3325);
                if (exitedEvent != null)
                {
                    g_ExitCode = proc.ExitCode;
                    exitedEvent(proc.ExitCode);
                }
                g_Proc = null;
            }
        }

        private String g_UserName = "";
        private String g_Password = "";
        private String g_Domain = "";
        public void setCreds(String username, String password, String domain)
        {
            g_UserName = username;
            g_Password = password;
            g_Domain = domain;
        }

        public void wait()
        {
            while(g_Proc != null)
            {
                Thread.Sleep(120);
            }
        }

        public int terminate()
        {
            int status = -1;
            if(g_Proc != null)
            {
                g_Proc.Kill();
            }
            wait();
            return status;
        }

        public int runCmd(String cmd, String args = "", String workingDir = "")
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
                g_Proc = new System.Diagnostics.Process();
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
                    while (g_Proc != null && !g_Proc.HasExited && !g_Async)
                        Thread.Sleep(120);
                    if (!g_Async)
                        result = "Finished.";
                    else
                        result = "Running...";
                    if (g_Async)
                    {
                        g_Proc.BeginErrorReadLine();
                        g_Proc.BeginOutputReadLine();
                    }
                }
                catch (Win32Exception e)
                {
                    result = "EXE Error: " + e.Message;
                    result += "\n" + e.StackTrace;
                    result += "\n" + cmd;
                    result += "\n" + args;
                    result += "\n" + workingDir;
                    //g_Stop = true;
                }
                catch (Exception ex)
                {
                    result = "EXE Error: " + ex.Message;
                    result += "\n" + ex.StackTrace;
                    result += "\n" + cmd;
                    result += "\n" + args;
                    result += "\n" + workingDir;
                    //g_Stop = true;
                }
                try
                {
                    if (g_Proc != null && g_Proc.HasExited)
                    {
                        status = g_Proc.ExitCode;
                    }
                    else
                    {
                        status = g_ExitCode;
                    }
                }
                catch (Exception ex)
                {
                }
                System.Console.WriteLine("Result: " + result);
                //System.Console.ReadKey();
            }
            if(!g_Async)
                g_Proc = null;
            return status;
        }
    }
}
