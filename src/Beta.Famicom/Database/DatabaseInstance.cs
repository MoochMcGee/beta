using System.Collections.Generic;
using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    [XmlRoot("database")]
    public sealed class DatabaseInstance
    {
        [XmlAttribute("version")]
        public string version { get; set; }

        [XmlAttribute("conformance")]
        public string conformance { get; set; }

        [XmlAttribute("author")]
        public string author { get; set; }

        [XmlAttribute("agent")]
        public string agent { get; set; }

        [XmlAttribute("timestamp")]
        public string timestamp { get; set; }

        [XmlElement("game")]
        public List<Game> games { get; set; }
    }
}
