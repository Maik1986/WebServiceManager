using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;
using WebServiceManager.Logic.Handler;
using System.Net;
using System.Data.SqlClient;
using System.Data;
using WebServiceManager.CrossCutting.Helpers;
using WebServiceManager.CrossCutting.Interfaces;
using WebServiceManager.Data.Storage;
using System.Net.Http.Headers;
using System.IO;

namespace WebServiceManager.Infrastructure.Communication
{
    public class UpdateFacade
    {
        private DBContext db;
        public  IHandler handler;
        public  ArchiveFacade archiveFacade;
        private HttpResponseMessage response;
        public String errorMessage;
        public HttpStatusCode errorCode;
        public string svcAppPath { get; set; }

        public UpdateEntry create(string websitename)
        {
            var dataObj = Serialization.Serialize(archiveFacade.archive.files);
            if (svcAppPath.Last() == '\\')
                svcAppPath = svcAppPath.TrimEnd('\\');
            var entry = db.create(dataObj, svcAppPath, websitename);
            return entry;
        }

        public List<UpdateEntry> read()
        {
            var allEntries = db.read(false);
            return allEntries;
        }
        public UpdateEntry read(int id)
        {
            var entry = db.read(id, true);
            return entry;
        }
        public HttpResponseMessage update(int id)
        {
            var allEntries = db.read(false);
            
            if ((allEntries.Where(m => m.id == id)).Count() < 1)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            // skip all entries, where updated field is null and where id is not the current one
            var entriesToSkip = allEntries.Where(m => m.updated == DateTime.MinValue).Where(y => y.id != id).ToList();

            db.update(id, entriesToSkip);
            response.StatusCode = HttpStatusCode.NoContent;
            return response;
        }
        public HttpResponseMessage delete(int id)
        {
            db.delete(id);
            response.StatusCode = HttpStatusCode.NoContent;
            return response;
        }

        public async Task readHandler()
        {
            await handler.read();
            if (handler.errorMessage != null)
            {
                errorMessage = handler.errorMessage;
                errorCode = handler.errorCode;
            }
        }
        public void createArchiveFacade(ArchiveFacade.archiveType archiveType)
        {
            archiveFacade = new ArchiveFacade(handler, archiveType);
            archiveFacade.archive.checkArchive();
            if (archiveFacade.archive.errorMessage == null)
            {
                archiveFacade.archive.convert();
            }

            if (archiveFacade.archive.errorMessage != null)
            {
                errorMessage = archiveFacade.archive.errorMessage;
                errorCode = archiveFacade.archive.errorCode;
            }
        }
        public UpdateFacade()
        {
            db = new DBContext();
            response = new HttpResponseMessage();
        }

        public UpdateFacade(HttpContent requestContent, Guid svcAssemblyGuid, string svcAppPath)
            : this()
        {
            this.svcAppPath = svcAppPath;
            handler = new MultipartFormDataContentHandler(requestContent, svcAssemblyGuid);
            
        }

        // implement further input object handlers here...
        // example:

        // public UpdateFacade(string requestContent, Guid svcAssemblyGuid)
        //      : this()
        // {
        //     this.handler = new MyNewStringHandler(requestContent, svcAssemblyGuid);
        // }

    }
}
