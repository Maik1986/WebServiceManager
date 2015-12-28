using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Threading.Tasks;
using System.Reflection;
using WebServiceManager.Infrastructure.Communication;
using WebServiceManager.Data.Storage;
using System.IO;
using System.Web.Hosting;


namespace WebServiceManager.UI.WebClient.Controllers
{
    public class UpdateController : ApiController
    {
        public string updatePathTest { get; private set; }
        public HttpResponseMessage response { get; set; }
        private UpdateFacade updateFacade { get; set; }

        // GET api/update
        /// <summary>
        /// List all uploaded versions "api/update" <br/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UpdateEntry> Get()
        {
            // return list of uploaded updates
            return updateFacade.read();
        }

        // GET api/update/5 - details of specific version
        /// <summary>
        /// Show details of specific version "api/update/1" <br/>
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Get(int id)
        {
            UpdateEntry responseObj = updateFacade.read(id);
            if (responseObj.errorMessage != null)
            {
                response = Request.CreateResponse(responseObj.errorCode);
                response.Content = new StringContent(responseObj.errorMessage);
            }
            else
            {
                response = Request.CreateResponse<UpdateEntry>
                    (HttpStatusCode.OK, responseObj);
            }
            return response;
        }

        // POST api/Update
        /// <summary>
        /// update to newer version (send update package) to "api/update" <br/>
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Post()
        {
            string currentPath;
            string websitename; 

            #region Set update path for testing purposes, if AppDomainAppPath is null.
            try
            {
                currentPath = HttpRuntime.AppDomainAppPath;
            }
            catch
            {
                if (updatePathTest == null)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Content = new StringContent("Either no update path found in the current http context or not explicitly set.");
                    return response;
                }
                else
                {
                    currentPath = updatePathTest;
                }
            }

            try
            {
                websitename = HostingEnvironment.ApplicationHost.GetSiteName();
            }
            catch(NullReferenceException)
            {
                websitename = "Test";
            }

            #endregion
            
            //System.Web.Hosting.HostingEnvironment.ApplicationHost.GetSiteName();
            
            // call constructor of UpdateFacade and pass through mandatory values for checking the archive
            updateFacade = new UpdateFacade(Request.Content,
                                        Assembly.GetExecutingAssembly().GetType().GUID,
                                        currentPath);
            await updateFacade.readHandler();
            if (updateFacade.errorMessage != null)
            {
                return buildError();
            }

            updateFacade.createArchiveFacade(ArchiveFacade.archiveType.Zip);
            if (updateFacade.errorMessage != null)
            {
                return buildError();
            }

            var responseObj = updateFacade.create(websitename);
            response = Request.CreateResponse<UpdateEntry>(HttpStatusCode.Created, responseObj);
                            
            return response;
        }
        // PUT api/update/5 - switch to different, available version (i.E. downgrade to previous version)
        /// <summary>
        /// Jump to another version, which has been uploaded already "api/update/1" <br/>
        /// </summary>
        /// <returns></returns>

        public HttpResponseMessage Put(int id)
        {
            return updateFacade.update(id);
        }

        // DELETE api/update/5 - delete specific version
        /// <summary>
        /// Delete specific version "api/update/1" <br/>
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Delete(int id)
        {
            return updateFacade.delete(id);
        }

        // builds error string/code and returns it
        private HttpResponseMessage buildError()
        {
            response.Content = new StringContent(updateFacade.errorMessage);
            response.StatusCode = updateFacade.errorCode;
            return response;
        }

        // This constructor is especially for testing.
        public UpdateController(string updatePathTest) : this()
        {
            this.updatePathTest = updatePathTest;
            
        }
        public UpdateController() {
            response = new HttpResponseMessage();
            updateFacade = new UpdateFacade();
        }
    }
}
