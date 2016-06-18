using System.Collections.Generic;
using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public class Game
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("altname")]
        public string AltName { get; set; }

        [XmlAttribute("class")]
        public string Class { get; set; }

        [XmlAttribute("subclass")]
        public string Subclass { get; set; }

        [XmlAttribute("catalog")]
        public string Catalog { get; set; }

        [XmlAttribute("publisher")]
        public string Publisher { get; set; }

        [XmlAttribute("developer")]
        public string Developer { get; set; }

        [XmlAttribute("portdeveloper")]
        public string PortDeveloper { get; set; }

        [XmlAttribute("players")]
        public int Players { get; set; }

        [XmlAttribute("date")]
        public string Date { get; set; }

        [XmlElement("cartridge")]
        public List<Cartridge> Cartridges { get; set; }
    }
}
