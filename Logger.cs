using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Utilities
{
    public enum MessageDestination
    {
        NONE = 0,
        CONSOLE = 1,
        FILE = 2,
        WINDOWS_EVENT_LOG = 3
    };

    public enum MessageSeverity
    {
        ERROR = 0,
        WARNING = 1,
        INFO = 2,
        DEBUG = 3
    };

    public class Logger
    {
        public static void log(string message,
            MessageSeverity logLevel,
            MessageDestination output = MessageDestination.CONSOLE)
        {
            log(message,
                (int)logLevel,
                (int)output);
        }

        public static void log(string message,
            int logLevel = (int)MessageSeverity.ERROR,
            int output = (int)MessageDestination.CONSOLE)
        {
            MessageDestination msgDest = (MessageDestination)output;
            MessageSeverity msgSrv = (MessageSeverity)logLevel;
            // line numbers and file/method names.
            // http://msdn.microsoft.com/en-us/library/system.diagnostics.stacktrace.aspx
            String fileName = "errorlog_" + DateTime.Now.ToString("yyyy-MM-dd") + "_%I%.txt";
            bool wroteToFile = false;

            String msg = "%TIME% %TYPE% " + message;
            msg = msg.Replace("%TIME%", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            if (System.Reflection.Assembly.GetEntryAssembly() == null)
            {
                // this means that we're being hosted in IIS or a web service
                msgDest = MessageDestination.WINDOWS_EVENT_LOG;
            }

            EventLogEntryType type = EventLogEntryType.Error;
            switch (msgSrv)
            {
                case MessageSeverity.ERROR:
                    msg = msg.Replace("%TYPE%", "Error");
                    type = EventLogEntryType.Error;
                    break;
                case MessageSeverity.WARNING:
                    msg = msg.Replace("%TYPE%", "Warning");
                    type = EventLogEntryType.Warning;
                    break;
                case MessageSeverity.INFO:
                    msg = msg.Replace("%TYPE%", "Info");
                    type = EventLogEntryType.Information;
                    break;
                case MessageSeverity.DEBUG:
                    msg = msg.Replace("%TYPE%", "Debug");
                    type = EventLogEntryType.Information;
#if !DEBUG
                    msgDest = MessageDestination.NONE;
#endif
                    break;
            }

            if (msgDest == MessageDestination.WINDOWS_EVENT_LOG)
            {
                // TODO : must be registered to write to the system log
                using (EventLog log = new EventLog())
                {
                    log.Source = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                    log.Log = "Application";
                    try
                    {
                        log.WriteEntry(msg, type);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            else if (msgDest == MessageDestination.FILE)
            {
                int i = 0;
                while (!wroteToFile)
                {
                    try
                    {
                        String appName = Process.GetCurrentProcess().ProcessName;
                        // String log = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        String path = System.IO.Path.GetTempPath() + "\\" +
                                   appName + "\\";
                                   fileName = path +
                                   fileName.Replace("%I%", i.ToString());
                        if (!File.Exists(fileName))
                        {
                            wroteToFile = true;
                            if(!String.IsNullOrEmpty(path))
                                Directory.CreateDirectory(path);
                        }

                        // TODO : PInvoke so we only need one file

                        FileStream file = new FileStream(fileName, FileMode.Append | FileMode.OpenOrCreate, FileAccess.Write);
                        StreamWriter sw = new StreamWriter(file);
                        sw.WriteLine(msg);
                        sw.Close();
                        file.Close();

                        wroteToFile = true;
                    }
                    catch (Exception e)
                    {
                        i++;
#if DEBUG
                        Console.WriteLine("Error in Utility.General::logError() " + e.Message);
#endif
                    }
                }
            }
            else if (msgDest == MessageDestination.CONSOLE)
            {
                Console.WriteLine(msg);
            }
        }
    }
}
