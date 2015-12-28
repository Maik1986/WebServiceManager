using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceManager.CrossCutting.Helpers
{
    [Serializable]
    public class ArchiveEntry : ISerializable
    {
        string nameInternal;
        public string name
        {
            get { return nameInternal; } 
            private set {
                path = Path.GetDirectoryName(value);
                filename = Path.GetFileName(value);
                } 
        }
        public string path { get; set; }
        public string filename { get; set; }

        public byte[] data { get; private set; }
        public Guid GUID { get; private set; }
        public Assembly library { get; private set; }
        public bool isLibrary { get; private set; }


        [SecurityPermission(SecurityAction.LinkDemand,
            Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(
        SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            info.AddValue("name", name);
            info.AddValue("data", data);
            info.AddValue("isLibrary", isLibrary);
        }

        public ArchiveEntry(string name, byte[] data)
        {
            this.name = name;
            nameInternal = name;
            this.data = data;
            isLibrary = false;
        }
        public ArchiveEntry(string name, byte[] data, Assembly library) : this(name, data)
        {
            this.GUID = library.GetType().GUID;
            this.library = library;
            isLibrary = true;
        }
        protected ArchiveEntry(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            name = (string)info.GetValue("name", typeof(string));
            data = (byte[])info.GetValue("data", typeof(byte[]));
            isLibrary = (bool)info.GetValue("isLibrary", typeof(bool));
        }   
    }
}
