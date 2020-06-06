using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace FolderCleanup
{
    class Logger
    {
        public static string logFileName;
        private static readonly int maxLogSize = int.Parse(ConfigurationManager.AppSettings["maxLogSize"]) * 1000;
        private static readonly int interval = int.Parse(ConfigurationManager.AppSettings["maintInterval"]) * 1000;
        private static readonly string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\";
        private static readonly Timer maintTimer = new Timer();

        static Logger()
        {
            maintTimer.Elapsed += new ElapsedEventHandler(LogMaintenance);
            maintTimer.Interval = interval;
            maintTimer.Enabled = true;
            if (Environment.UserInteractive)
            {
                logFileName = "ConsoleLog.txt";
            }
            else
            {
                logFileName = "ServiceLog.txt";
            }
        }

        public static void WriteMessage(string Message)
        {
            Message = "[" + DateTime.Now + "] " + Message;
            string filePath = path + logFileName;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            using (StreamWriter sw = new StreamWriter(filePath, true))
            {
                sw.WriteLine(Message);
            }
        }

        private static void LogMaintenance(object sender, ElapsedEventArgs e)
        {
            string filePath = path + logFileName;
            if (File.Exists(filePath))
            {
                long length = new FileInfo(filePath).Length;
                if (length > maxLogSize)
                {
                    List<string> lines = new List<string>();
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        string line = String.Empty;
                        long leftToRemove = length - maxLogSize;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (leftToRemove > 0)
                            {
                                leftToRemove -= line.Length;
                            }
                            else
                            {

                                lines.Add(line);
                            }
                        }
                    }
                    using (StreamWriter sw = new StreamWriter(filePath, false))
                    {
                        foreach (string line in lines)
                        {
                            sw.WriteLine(line);
                        }
                    }
                    decimal newLen = new FileInfo(filePath).Length / 1000;
                    WriteMessage("LOGMAINT: Completed. Ending file size is " + Math.Round(newLen) + "KB");
                }
                else
                {
                    WriteMessage("LOGMAINT: File size is below threshold");
                }
            }
        }
    }
}
