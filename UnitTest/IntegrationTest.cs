using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Net.Http;
using WebServiceManager.UI.WebClient.Controllers;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebServiceManager.Logic.Update;
using System.Timers;
using System.Web.Http;
using Newtonsoft.Json;
using WebServiceManager.Data.Storage;

namespace WebServiceManager.Tests
{
    [TestClass]
    public class IntegrationTest
    {
        [TestMethod]
        public async Task TestUpload()
        {
            DBContext db = new DBContext();
            
            MultipartFormDataContent formDataContent = new MultipartFormDataContent();
            StreamContent file1 = new StreamContent(File.OpenRead(@"C:\Users\D063169\Desktop\WebClient2.zip"));
            file1.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            file1.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
            file1.Headers.ContentDisposition.FileName = "WebClient.zip";
            formDataContent.Add(file1);
            
            var updateController = new UpdateController("C:\\Temp\\TestUpdate");
            
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = formDataContent;
            request.Headers.Add("Accept", "application/json");
            request.Method = HttpMethod.Post;
            request.SetConfiguration(new HttpConfiguration());

            updateController.Request = request;
            var response = await updateController.Post();

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObj = JsonConvert.DeserializeObject<UpdateEntry>(responseContent);
            
            //check, if returned representation has been added to the database successfully.
            var dbEntry = db.read(responseObj.id, false);
            Assert.IsNull(dbEntry.errorMessage);
        }

        [TestMethod]
        public void TestRead()
        {
            var updateController = new UpdateController();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.Headers.Add("Accept", "application/xml");
            updateController.Request = request;
            var response = updateController.Get();
            //response.EnsureSuccessStatusCode();

        }

        [TestMethod]
        public void TestPut()
        {
            var updateController = new UpdateController();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Put;
            //request.
            updateController.Request = request;
            var response = updateController.Put(13);
            response.EnsureSuccessStatusCode();

        }
        [TestMethod]
        public void TestDelete()
        {
            var updateController = new UpdateController();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Delete;
            updateController.Request = request;
            var response = updateController.Delete(13);
            response.EnsureSuccessStatusCode();

        }

        [TestMethod]
        public void TestUpdate()
        {
            Update update = new Update();
            //Timer schedule = new Timer();
            update.update(null, null);
            
            /*schedule.Interval = 1;
            schedule.AutoReset = false;
            schedule.Elapsed += new ElapsedEventHandler(update.checkForUpdate);
            schedule.Start();*/
        }
    }
}
