using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WebServiceManager.CrossCutting.Helpers;
using WebServiceManager.Data.Archive;

namespace WebServiceManager.Data.Storage
{
    [DataContract]
    public class UpdateEntry
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string path { get; set; }
        [DataMember]
        public DateTime inserted { get; set; }
        [DataMember]
        public DateTime updated { get; set; }
        public string websitename { get; set; }
        public string errorMessage { get; private set; }
        public HttpStatusCode errorCode { get; private set; }

        public List<ArchiveEntry> files { get; set; }

        public UpdateEntry() { }
        public UpdateEntry(int id, string path)
        {
            this.id = id;
            this.path = path;

        }
        public UpdateEntry(int id, string path, DateTime inserted) 
            : this(id, path)
        {
            this.inserted = inserted;
        }
        public UpdateEntry(int id, string path, DateTime inserted, DateTime updated) 
            : this(id, path, inserted)
        {
            this.updated = updated;
        }
        public UpdateEntry(int id, string path, DateTime inserted, DateTime updated, List<ArchiveEntry> files)
            : this(id, path, inserted, updated)
        {
            this.files = files;
        }
        public UpdateEntry(int id, string path, object inserted, object updated, object files, object websitename)
            : this(id, path)
        {
            this.updated = updated.GetType() == typeof(DBNull) ? DateTime.MinValue : (DateTime)updated;
            this.inserted = inserted.GetType() == typeof(DBNull) ? DateTime.MinValue : (DateTime)inserted;
            this.websitename = websitename.GetType() == typeof(String) ? (string)websitename : null;

            if (files.GetType() == typeof(byte[]))
            {
                this.files = (List<ArchiveEntry>)Serialization.Deserialize((byte[])files);
            }
        }
        // create in case of issues
        public UpdateEntry(string errorMessage, HttpStatusCode errorCode)
        {
            this.errorMessage = errorMessage;
            this.errorCode = errorCode;
        }
    }
}
