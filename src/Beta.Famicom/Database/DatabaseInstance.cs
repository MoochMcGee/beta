using System.Collections.Generic;
using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    [XmlRoot("database")]
    public sealed class DatabaseInstance
    {
        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlAttribute("conformance")]
        public string Conformance { get; set; }

        [XmlAttribute("author")]
        public string Author { get; set; }

        [XmlAttribute("agent")]
        public string Agent { get; set; }

        [XmlAttribute("timestamp")]
        public string Timestamp { get; set; }

        [XmlElement("game")]
        public List<Game> Games { get; set; }
    }
}
