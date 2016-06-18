using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public class Pin
    {
        [XmlAttribute("number")]
        public int Number { get; set; }

        [XmlAttribute("function")]
        public string Function { get; set; }
    }
}
