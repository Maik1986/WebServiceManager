using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServiceManager.CrossCutting.Interfaces;
using WebServiceManager.Data.Archive;

namespace WebServiceManager.Infrastructure.Communication
{
    public class ArchiveFacade
    {
        public IArchive archive;
        public IHandler handler;

        public enum archiveType
        {
            Zip,
            //Rar
        }

        public ArchiveFacade(IHandler handler, archiveType atype)
        {
            this.handler = handler;
            if (atype == archiveType.Zip)
            {
                archive = new ZipArchive(handler);
            }

            // implement further archive handlers here...
            // example:
            // else if (atype == archiveType.Rar)
            // {
            //     archive = new MyNewRarArchive(handler);
            // }
        }
    }
}
