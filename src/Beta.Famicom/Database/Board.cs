using System.Collections.Generic;
using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public class Board
    {
        [XmlAttribute("type")]
        public string type { get; set; }

        [XmlAttribute("pcb")]
        public string name { get; set; }

        [XmlAttribute("mapper")]
        public int mapper { get; set; }

        [XmlElement("prg")]
        public List<Rom> prg { get; set; }

        [XmlElement("chr")]
        public List<Rom> chr { get; set; }

        [XmlElement("wram")]
        public List<Ram> wram { get; set; }

        [XmlElement("vram")]
        public List<Ram> vram { get; set; }

        [XmlElement("chip")]
        public List<Chip> chip { get; set; }

        [XmlElement("cic")]
        public List<Cic> cic { get; set; }

        [XmlElement("pad")]
        public Pad solderPad { get; set; }
    }
}
