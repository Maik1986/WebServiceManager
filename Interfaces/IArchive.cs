using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebServiceManager.CrossCutting.Helpers;


namespace WebServiceManager.CrossCutting.Interfaces
{
    public interface IArchive
    {
        IHandler handler { get; set; }
        HttpStatusCode errorCode { get; }
        String errorMessage { get; }
        List<ArchiveEntry> files { get; set; }
        void convert();
        void checkAssembly();
        void checkArchive();
    }
}
