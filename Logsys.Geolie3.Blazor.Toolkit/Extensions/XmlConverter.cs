using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ERP.DEMO.Toolkit.Extensions
{
    public class XmlConverter
    {
        public string SerializeObjectToXml<T>(T model)
        {
            if (model == null) return null;
            XmlSerializer xmlSerializer = new XmlSerializer(model.GetType());
            StringWriter writer = new StringWriter();
            xmlSerializer.Serialize(writer, model);
            return writer.ToString();
        }

        public T DeserializeXmlToObject<T>(string xml)
        {
            if (xml == null || xml.IsJsonNullOrEmpty()) return default(T);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            byte[] byteArray = Encoding.Unicode.GetBytes(xml);
            MemoryStream stream = new MemoryStream(byteArray);
            T Object = (T)xmlSerializer.Deserialize(stream); 
            return Object;
        }
    }
}