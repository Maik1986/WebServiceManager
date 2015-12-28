using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Net.Http.Formatting;
using System.Web;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Http.Headers;
using WebServiceManager.CrossCutting.Interfaces;

namespace WebServiceManager.Logic.Handler
{
    public class MultipartFormDataContentHandler : IHandler
    {
        public HttpStatusCode errorCode { get; private set; }
        public String errorMessage { get; private set; }
        public Guid svcAssemblyGuid { get; set; }
        private HttpContent requestContent { get; set; }
        public Stream inputStream { get; set; }
        public MultipartFormDataContentHandler(HttpContent requestContent, Guid svcAssemblyGuid)
        {
            this.requestContent = requestContent;
            this.svcAssemblyGuid = svcAssemblyGuid;
        }

        public async Task read()
        {
            if (requestContent.IsMimeMultipartContent())
            {
                // first, copy stream, to check it in memory
                MultipartMemoryStreamProvider memStream;
                memStream = await requestContent.ReadAsMultipartAsync();
                if (memStream.Contents.Count == 1)
                {
                    inputStream = await memStream.Contents.FirstOrDefault().ReadAsStreamAsync();
                    inputStream.Position = 0;
                }
                else
                {
                    errorMessage = "Too many contents. Only one file is allowed.";
                    errorCode = HttpStatusCode.BadRequest;
                }
            }
            else
            {
                errorMessage = "Media type is not multipart/form-data. Consider to add this to the Content-Type header.";
                errorCode = HttpStatusCode.NotImplemented;
            }
        }
    }
}
