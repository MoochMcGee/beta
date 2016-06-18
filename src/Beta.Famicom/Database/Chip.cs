using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public class Chip : IC
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("battery")]
        public bool Battery { get; set; }
    }
}
