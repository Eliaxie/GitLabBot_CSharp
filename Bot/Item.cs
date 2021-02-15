using System.Xml.Serialization;

namespace Bot
{
    public class item
    {
        [XmlAttribute]
        public string id_document;
        [XmlAttribute]
        public string path_document;
    }
}