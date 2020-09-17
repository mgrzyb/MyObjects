using System.IO;
using System.Xml;
using System.Xml.Serialization;
using NHibernate.Cfg.MappingSchema;

namespace MyObjects
{
    public static class HbmMappingExtensions
    {
        static string ToXmlString(this HbmMapping mapping)
        {
            var setting = new XmlWriterSettings {Indent = true};
            var serializer = new XmlSerializer(typeof(HbmMapping));
            using var memStream = new MemoryStream(2048);
            using var xmlWriter = XmlWriter.Create(memStream, setting);
            serializer.Serialize(xmlWriter, mapping);
            memStream.Flush();
            memStream.Position = 0;
            using var sr = new StreamReader(memStream);
            return sr.ReadToEnd();
        }
    }
}