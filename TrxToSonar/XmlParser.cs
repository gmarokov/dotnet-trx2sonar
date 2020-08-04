using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TrxToSonar
{
    public class XmlParser<T>
    {
        private readonly XmlSerializer _xmlSerializer = new XmlSerializer(typeof(T));

        public bool Save(T xmlDocument, string outputFilename)
        {
            string xmlContent = Serialize(xmlDocument);

            if (string.IsNullOrEmpty(xmlContent))
            {
                return false;
            }

            var fileInfo = new FileInfo(outputFilename);

            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }
            else if (fileInfo.Directory?.Exists == false)
            {
                fileInfo.Directory.Create();
            }

            File.WriteAllText(outputFilename, xmlContent);
            return true;
        }

        public T Deserialize(string filename)
        {
            using var streamReader = new StreamReader(filename);
            return (T) _xmlSerializer.Deserialize(streamReader);
        }

        private string Serialize(T xmlDocument)
        {
            var emptyNamespaces = new XmlSerializerNamespaces();
            emptyNamespaces.Add("", "");

            var xmlSettings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };

            using var streamWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(streamWriter, xmlSettings);
            _xmlSerializer.Serialize(xmlWriter, xmlDocument, emptyNamespaces);
            return streamWriter.ToString();
        }
    }
}
