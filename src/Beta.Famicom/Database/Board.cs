using System.Collections.Generic;
using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public class Board
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("pcb")]
        public string Name { get; set; }

        [XmlAttribute("mapper")]
        public int Mapper { get; set; }

        [XmlElement("prg")]
        public List<Rom> Prg { get; set; }

        [XmlElement("chr")]
        public List<Rom> Chr { get; set; }

        [XmlElement("wram")]
        public List<Ram> Wram { get; set; }

        [XmlElement("vram")]
        public List<Ram> Vram { get; set; }

        [XmlElement("chip")]
        public List<Chip> Chip { get; set; }

        [XmlElement("cic")]
        public List<Cic> Cic { get; set; }

        [XmlElement("pad")]
        public Pad SolderPad { get; set; }
    }
}
