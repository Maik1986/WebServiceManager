using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceManager.CrossCutting.Interfaces
{
    public interface IWebserver
    {
        void stopWebsite(string websitename);
        void startWebsite(string websitename);
    }
}
