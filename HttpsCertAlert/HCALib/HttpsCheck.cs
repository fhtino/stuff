using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HCALib
{
    public class HttpsCheck
    {
        [XmlAttribute]
        public string Url { get; set; }

        [XmlAttribute]
        public string Group { get; set; }

        [XmlAttribute]
        public bool Active { get; set; }

        [XmlAttribute]
        public int AlertDays { get; set; }

        [XmlAttribute]
        public DateTime LastCheckDT { get; set; }

        [XmlAttribute]
        public DateTime ValidToDT { get; set; }

        [XmlAttribute]
        public Status Status { get; set; }  // OK, EXPIRING, EXPIRED, ERROR

        [XmlAttribute]
        public string LastError { get; set; }
    }
}
