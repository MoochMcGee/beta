using System.Collections.Generic;
using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public class IC
    {
        [XmlElement("pin")]
        public List<Pin> Pins { get; set; }
    }
}
