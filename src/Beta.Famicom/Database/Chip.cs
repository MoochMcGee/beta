using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public class Chip : IC
    {
        [XmlAttribute("type")]
        public string type { get; set; }

        [XmlAttribute("battery")]
        public bool battery { get; set; }
    }
}
