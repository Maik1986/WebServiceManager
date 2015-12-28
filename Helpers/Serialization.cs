using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WebServiceManager.CrossCutting.Helpers
{
    public class Serialization
    {
        public static byte[] Serialize(object obj)
        {
            using (Stream stream = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(stream, obj);
                byte[] b = null;
                b = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(b, 0, (int)stream.Length);
                stream.Close();
                return b;
            }
        }

        public static object Deserialize(byte[] b)
        {
            using (MemoryStream stream = new MemoryStream(b))
            {
                BinaryFormatter bf = new BinaryFormatter();
                object obj = bf.Deserialize(stream);
                stream.Close();
                return obj;
            }
        }

        /*public static String GetXml(List<UpdateEntry> data)
        {
            XmlSerializer xml = new XmlSerializer(typeof(List<UpdateEntry>));
            StringWriter stringWriter = new StringWriter();
            xml.Serialize(stringWriter, data);
            return stringWriter.ToString();
        }*/
    }
}
