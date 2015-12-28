using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using WebServiceManager.CrossCutting.Interfaces;
using WebServiceManager.CrossCutting.Helpers;

namespace WebServiceManager.Data.Archive
{
    /// <summary>
    /// This class is intended for actions within archives and included libraries
    /// </summary>
    ///
    [Serializable]
    public class ZipArchive : IArchive
    {
        public HttpStatusCode errorCode { get; private set; }
        public String errorMessage { get; private set; }
        public List<ArchiveEntry> files { get; set; }
        public  IHandler handler { get; set; }

        public string libraryExtention { get; private set; }

        /// <summary>
        /// This function extracts an archive out of a stream and returns all Assembly GUIDS of the included libraries.
        /// </summary>
        /// 
        /// <returns>A list of type Guid</returns>
        public void convert()
        {
            List<Guid> includedAssemblyGUIDs = new List<Guid>();

            using (ZipInputStream s = new ZipInputStream(handler.inputStream))
            {
                ZipEntry fileInArchive;
                while ((fileInArchive = s.GetNextEntry()) != null)
                {
                    Console.WriteLine(fileInArchive.Name);
                    byte[] aData = new byte[s.Length];
                    s.Read(aData, 0, (int)s.Length);

                    if (fileInArchive.Name.EndsWith(libraryExtention))
                    {
                        Assembly paketAssembly = Assembly.Load(aData);
                        files.Add(new ArchiveEntry(fileInArchive.Name, aData, paketAssembly));
                        
                    }
                    else
                    {
                        files.Add(new ArchiveEntry(fileInArchive.Name, aData));
                    }
                }
            }
            // check, if uploaded data fits
            checkAssembly();
        }
        public void checkAssembly()
        {
            var items = files.Where(m => m.GUID == handler.svcAssemblyGuid);
            if(items.Count() <= 0)
            {
                errorCode = HttpStatusCode.BadRequest;
                errorMessage = "Content of archive is not valid. Please check if it is intended for updating this web service.";
            }
        }
        public void checkArchive()
        {
            ZipInputStream s = new ZipInputStream(handler.inputStream);
            try
            {
                s.GetNextEntry();
            }
            catch
            {
                errorCode = HttpStatusCode.NotImplemented;
                errorMessage = "Type of archive or file is not supported.";
            }

            handler.inputStream.Position = 0;
        }
        
        public ZipArchive(IHandler handler)
        {
            libraryExtention = Settings.Default.libraryExtention;
            this.handler = handler;
            files = new List<ArchiveEntry>();
        }
    }
}
