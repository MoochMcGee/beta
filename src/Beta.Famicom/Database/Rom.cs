using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public class Rom : IC
    {
        [XmlIgnore]
        public int Size { get; private set; }

        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("size")]
        public string SizeString
        {
            get { return (Size / 1024) + "k"; }
            set { Size = int.Parse(value.Substring(0, value.Length - 1)) * 1024; }
        }

        [XmlAttribute("file")]
        public string File { get; set; }

        [XmlAttribute("crc")]
        public string Crc { get; set; }

        [XmlAttribute("sha1")]
        public string Sha1 { get; set; }
    }
}
