using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using WebServiceManager.CrossCutting.Helpers;
using WebServiceManager.CrossCutting.Interfaces;
using WebServiceManager.Data.Storage;
using WebServiceManager.Logic.Webserver;

namespace WebServiceManager.Logic.Update
{
    public partial class Update : ServiceBase
    {
        private List<UpdateEntry> updateEntries;
        private Timer schedule;
        private DBContext db;
        private IWebserver webserver;
        public Update()
        {
            updateEntries = new List<UpdateEntry>();
            db = new DBContext();
            webserver = new IIS();
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            schedule = new Timer();
            schedule.Interval = 15000;
            schedule.Elapsed += new ElapsedEventHandler(update);
            schedule.Enabled = true;
            // schedule.Start();
        }

        protected override void OnStop()
        {
            schedule.Enabled = false;
        }
        
        public void update(object sender, ElapsedEventArgs e) 
        {
            updateEntries = db.read(true, "Updated is null");

            if (updateEntries.Count > 0)
            {
                // if multiple update entries available, take newest 
                if (updateEntries.Count > 1)
                {
                    updateEntries = updateEntries.OrderByDescending(m => m.inserted).ToList();
                    // set older updates to date in the past
                    for (int i = 1; i < updateEntries.Count; i++)
                    {
                        db.updateUpdatedColumn(updateEntries[i].id, DateTime.Parse("01.01.1990"));
                    }
                }
                webserver.stopWebsite(updateEntries.FirstOrDefault().websitename);
                doUpdate(updateEntries.FirstOrDefault());
                webserver.startWebsite(updateEntries.FirstOrDefault().websitename);

                // set current date for updated archive
                db.updateUpdatedColumn(updateEntries.FirstOrDefault().id, DateTime.Now);
            }
        }
        private void doUpdate(UpdateEntry toUpdate)
        {
            var rootPath = toUpdate.path;
            if (Directory.Exists(rootPath))
            {
                //create backup of current version
                string bkpPath = rootPath + "_bpk_" + 
                    DateTime.Now.Year + 
                    DateTime.Now.Month + 
                    DateTime.Now.Day + 
                    DateTime.Now.Hour + 
                    DateTime.Now.Minute + 
                    DateTime.Now.Second;
                Directory.Move(rootPath, bkpPath);
                Directory.CreateDirectory(rootPath);
            }

            foreach (var file in toUpdate.files)
            {
                if (!String.IsNullOrEmpty(file.path))
                {
                    var pathSplit = file.path.Split('\\');
                    rootPath = toUpdate.path + "\\";
                    for (int i = 0; i < pathSplit.Length; i++)
                    {
                        if(!Directory.Exists(rootPath + pathSplit[i]))
                        {
                            Directory.CreateDirectory(rootPath + pathSplit[i]);
                        }
                        rootPath = rootPath + pathSplit[i] + "\\";
                    }
                }
                if (!String.IsNullOrEmpty(file.filename))
                {
                    string fileAndPath;
                    if (String.IsNullOrEmpty(file.path))
                    {
                        fileAndPath = file.filename;
                    }
                    else
                    {
                        fileAndPath = file.path + "\\" + file.filename;
                    }
                    using (var fileStream = File.Create(toUpdate.path + "\\" + fileAndPath))
                    {
                        fileStream.Write(file.data, 0, file.data.Length);
                    }
                }
            }
        }
    }
}
