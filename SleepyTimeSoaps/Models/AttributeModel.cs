using System.Collections.Generic;
using System.Xml;

namespace SleepyTimeSoaps.Models
{
    public class AttributeModel
    {
        public string AttributeName { get; set; }

        public XmlDocument BaseData { get; set; }

        public List<string> _Values = new List<string>();
        public List<string> Values { get { return _Values; } }

        public List<string> _Descriptions = new List<string>();
        public List<string> Descriptions { get { return _Descriptions; } }

        public List<int> _Prices = new List<int>();
        public List<int> Prices { get { return _Prices; } }
    }
}