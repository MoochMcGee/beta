using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public class Cic
    {
        [XmlAttribute("type")]
        public string Type { get; set; }
    }
}
