using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public class Rom : IC
    {
        [XmlIgnore]
        public int size { get; private set; }

        [XmlAttribute("id")]
        public int id { get; set; }

        [XmlAttribute("name")]
        public string name { get; set; }

        [XmlAttribute("size")]
        public string sizeString
        {
            get { return (size / 1024) + "k"; }
            set { size = int.Parse(value.Substring(0, value.Length - 1)) * 1024; }
        }

        [XmlAttribute("file")]
        public string file { get; set; }

        [XmlAttribute("crc")]
        public string crc { get; set; }

        [XmlAttribute("sha1")]
        public string sha1 { get; set; }
    }
}
