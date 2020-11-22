using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace FolderCleanup
{
    public partial class Service : ServiceBase
    {
        private readonly int maxDays = int.Parse(ConfigurationManager.AppSettings["fileRetainDays"]);
        private readonly string folderPath = ConfigurationManager.AppSettings["folderToCleanup"];
        private readonly Timer serviceTimer = new Timer();
        private int poll_counter = 0;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.AddMessage("SERVICE_STARTED");
            serviceTimer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            serviceTimer.Interval = int.Parse(ConfigurationManager.AppSettings["pollInterval"]) * 1000;
            serviceTimer.Start();
        }

        private void OnElapsedTime(object sender, ElapsedEventArgs e)
        {
            serviceTimer.Stop();
            poll_counter = poll_counter == 999 ? 1 : poll_counter + 1;

            Logger.AddMessage($"FOLDER_CLEANUP_STARTING_{poll_counter}");

            if (Directory.Exists(folderPath))
            {
                int count = 0;
                string[] files = Directory.GetFiles(folderPath).ToArray();

                foreach (string file in files)
                {
                    DateTime fileLastMod = File.GetCreationTime(file);
                    if ((DateTime.Today - fileLastMod).Days > maxDays)
                    {
                        File.Delete(file);
                        count++;
                    }
                }

                Logger.AddMessage($"Removed {count} files.");
            }
            else
            {
                Logger.AddMessage("EXCEPTION_DIRECTORY_NOT_FOUND: " + folderPath);
            }

            Logger.AddMessage($"FOLDER_CLEANUP_COMPLETE_{poll_counter}");

            serviceTimer.Start();
        }

        protected override void OnStop()
        {
            Logger.WriteMessageNoWait("SERVICE_STOPPED");
        }

        public Boolean IsInstalled()
        {
            Boolean installed = ServiceController.GetServices().Any(s => s.ServiceName == ConfigurationManager.AppSettings["serviceName"]);
            return installed;
        }
    }
}
