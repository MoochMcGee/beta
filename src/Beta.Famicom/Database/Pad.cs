using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public class Pad
    {
        [XmlAttribute("h")]
        public int h { get; set; }

        [XmlAttribute("v")]
        public int v { get; set; }
    }
}
