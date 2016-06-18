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
        public string System { get; set; }

        [XmlAttribute("revision")]
        public string Revision { get; set; }

        [XmlAttribute("prototype")]
        public string Prototype { get; set; }

        [XmlAttribute("dumper")]
        public string Dumper { get; set; }

        [XmlAttribute("datedumped")]
        public string DateDumped { get; set; }

        [XmlAttribute("dump")]
        public string Dump { get; set; }

        [XmlAttribute("crc")]
        public string CrcString
        {
            get { return crc.ToString("X8"); }
            set { crc = int.Parse(value, NumberStyles.HexNumber); }
        }

        [XmlAttribute("sha1")]
        public string Sha1 { get; set; }

        [XmlElement("board")]
        public List<Board> Boards { get; set; }
    }
}
