using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public class Pin
    {
        [XmlAttribute("number")]
        public int number { get; set; }

        [XmlAttribute("function")]
        public string function { get; set; }
    }
}
