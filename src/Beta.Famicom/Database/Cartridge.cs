using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public class Cartridge
    {
        [XmlIgnore]
        private int crc;

        [XmlAttribute("system")]
        public string system { get; set; }

        [XmlAttribute("revision")]
        public string revision { get; set; }

        [XmlAttribute("prototype")]
        public string prototype { get; set; }

        [XmlAttribute("dumper")]
        public string dumper { get; set; }

        [XmlAttribute("datedumped")]
        public string dateDumped { get; set; }

        [XmlAttribute("dump")]
        public string dump { get; set; }

        [XmlAttribute("crc")]
        public string crcString
        {
            get { return crc.ToString("X8"); }
            set { crc = int.Parse(value, NumberStyles.HexNumber); }
        }

        [XmlAttribute("sha1")]
        public string sha1 { get; set; }

        [XmlElement("board")]
        public List<Board> boards { get; set; }
    }
}
