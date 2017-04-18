using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public class Ram : Rom
    {
        [XmlAttribute("battery")]
        public bool battery { get; set; }
    }
}
