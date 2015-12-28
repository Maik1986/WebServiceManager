using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebServiceManager.CrossCutting.Interfaces;


namespace WebServiceManager.Logic.Webserver
{
    public class IIS : IWebserver
    {
        public ServerManager server { get; set; }
        public IIS()
        {
            server = new ServerManager();
        }
        public void startWebsite(string websitename) 
        {
            var site = server.Sites.FirstOrDefault(s => s.Name == websitename);
            if (site != null)
            {
                if (site.State != ObjectState.Started)
                    site.Start();
                if (site.State != ObjectState.Started)
                    throw new InvalidOperationException("Could not start website!");

                foreach (var application in site.Applications)
                {
                    var appPool = server.ApplicationPools.FirstOrDefault(a => a.Name == application.ApplicationPoolName);
                    if (appPool.State != ObjectState.Started)
                        appPool.Start();
                    if (appPool.State != ObjectState.Started)
                        throw new InvalidOperationException("Could not start application pool!");
                }
            }
            else
            {
                throw new InvalidOperationException("Could not find website!");
            }
        }
        public void stopWebsite(string websitename)
        {
            
            var site = server.Sites.FirstOrDefault(s => s.Name == websitename);
            
            if (site != null)
            {
                if (site.State != ObjectState.Stopped)
                    site.Stop();
                if (site.State != ObjectState.Stopped)
                    throw new InvalidOperationException("Could not stop website!");

                foreach (var application in site.Applications) 
                {
                    var appPool = server.ApplicationPools.FirstOrDefault(a => a.Name == application.ApplicationPoolName);
                    if (appPool.State != ObjectState.Stopped)
                        appPool.Stop();
                    if (appPool.State != ObjectState.Stopped)
                        throw new InvalidOperationException("Could not stop application pool!");
                }
            }
            else
            {
                throw new InvalidOperationException("Could not find website!");
            }

            //let IIS release all open file handles.
            Thread.Sleep(2000);
        }
    }
}
