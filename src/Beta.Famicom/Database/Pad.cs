using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public class Pad
    {
        [XmlAttribute("h")]
        public int H { get; set; }

        [XmlAttribute("v")]
        public int V { get; set; }
    }
}
