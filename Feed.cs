using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SiteAutomation
{
    [XmlRoot("rss"), Serializable]
    public class Feed
    {
        [XmlAttribute("version")]
        public readonly string Version = "2.0";
        [XmlElement("title")]
        public string Title { get; set; }
        [XmlElement("link")]
        public string Link { get; set; }
        [XmlIgnore]
        public DateTime Updated { get; set; } = DateTime.Now;
        [XmlElement("channel")]
        public Channel Items { get; set; }

        [Serializable]
        public class Channel
        {
            [XmlElement("item")]
            public List<ItemModel> ItemModels { get; set; }
        }
        public Feed() { }

        public void Serialize()
        {
            XmlSerializer           xmlSerializer         = new XmlSerializer(typeof(Feed));
            XmlSerializerNamespaces xSerializerNamespaces = new XmlSerializerNamespaces();

            xSerializerNamespaces.Add("g", "http://base.google.com/ns/1.0");

            using (StreamWriter stream = new StreamWriter(@"GoogleFeed.xml"))
            {
                xmlSerializer.Serialize(stream, this, xSerializerNamespaces);
            }
        }

        public Feed Deserialize()
        {
            string path = "Feeds/GoogleFeed.xml";

            XmlSerializer serializer = new XmlSerializer(typeof(Feed));

            using StreamReader reader = new StreamReader(path);
                return (Feed)serializer.Deserialize(reader);
            
        }

    }
}
