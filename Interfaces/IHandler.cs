using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace WebServiceManager.CrossCutting.Interfaces
{
    public interface IHandler
    {
        HttpStatusCode errorCode { get; }
        String errorMessage { get; }
        Stream inputStream { get; set; }
        Guid svcAssemblyGuid { get; set; }
        Task read();
    }
}
