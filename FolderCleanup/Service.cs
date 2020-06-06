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
        private readonly int interval = int.Parse(ConfigurationManager.AppSettings["pollInterval"]) * 1000;
        private readonly int maxDays = int.Parse(ConfigurationManager.AppSettings["fileRetainDays"]);
        private readonly string folderPath = ConfigurationManager.AppSettings["folderToCleanup"];
        private readonly Timer serviceTimer = new Timer();

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.WriteMessage("Service has started...");
            serviceTimer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            serviceTimer.Interval = interval;
            serviceTimer.Enabled = true;
        }

        private void OnElapsedTime(object sender, ElapsedEventArgs e)
        {
            if (Directory.Exists(folderPath))
            {
                Logger.WriteMessage("Starting folder clean-up");
                int count = 0;
                string[] files = Directory.GetFiles(folderPath).ToArray();
                foreach (string file in files)
                {
                    DateTime fileLastMod = File.GetLastAccessTime(file);
                    if ((DateTime.Today - fileLastMod).Days > maxDays)
                    {
                        File.Delete(file);
                        count++;
                    }
                }
                Logger.WriteMessage("Number of files deleted: " + count);
            }
            else
            {
                Logger.WriteMessage("DirectoryNotFound -- " + folderPath);
            }
        }

        protected override void OnStop()
        {
            Logger.WriteMessage("Service has stopped");
        }

        public void RunInConsole()
        {
            string[] args = new string[0];
            OnStart(args);
        }

        public void Pause()
        {
            serviceTimer.Enabled = false;
            Logger.WriteMessage("Service has been paused");
        }

        public void Resume()
        {
            serviceTimer.Enabled = true;
            Logger.WriteMessage("Service has resumed...");
        }

        public Boolean IsRunning()
        {
            return serviceTimer.Enabled;
        }

        public Boolean IsInstalled()
        {
            Boolean installed = ServiceController.GetServices().Any(s => s.ServiceName == ConfigurationManager.AppSettings["serviceName"]);
            return installed;
        }
    }
}
