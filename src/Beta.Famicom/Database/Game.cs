using System.Collections.Generic;
using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public class Game
    {
        [XmlAttribute("name")]
        public string name { get; set; }

        [XmlAttribute("altname")]
        public string altName { get; set; }

        [XmlAttribute("class")]
        public string @class { get; set; }

        [XmlAttribute("subclass")]
        public string subclass { get; set; }

        [XmlAttribute("catalog")]
        public string catalog { get; set; }

        [XmlAttribute("publisher")]
        public string publisher { get; set; }

        [XmlAttribute("developer")]
        public string developer { get; set; }

        [XmlAttribute("portdeveloper")]
        public string portDeveloper { get; set; }

        [XmlAttribute("players")]
        public int players { get; set; }

        [XmlAttribute("date")]
        public string date { get; set; }

        [XmlElement("cartridge")]
        public List<Cartridge> cartridges { get; set; }
    }
}
