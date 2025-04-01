using System.Net;
using System.Reflection;
using System.Xml;

namespace KUKA.RSI {
    /// <summary>
    /// Класс для создания конфигурации робота. 
    /// Тестировался мало, не гарантируется полноценная работа.
    /// </summary>
    public static class CONFIG {
        public const string TAG_DEF_TYPE = "INTERNAL";
        public static string DefaultNameSensor { get; set; } = "ImFree";
        public static string DefaultNameRobot { get; set; } = "KUKA";

        public static XmlDocument CreateXmlConfig(COMMON_CONFIG config)
            => CreateXmlConfig(config.CONFIG, config.SEND, config.RECEIVE);

        public static COMMON_CONFIG ParseConfigFromXmlConfig(string xml) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return ParseConfigFromXmlConfig(doc);
        }

        public static Dictionary<string, string> GetSendValueTable(COMMON_CONFIG config)
            => GetValueTable(config.RECEIVE.Select(x => (ITAG)x));

        public static Dictionary<string, string> GetValueTable(IEnumerable<CONFIG_DATA_RECEIVE> recs)
            => GetValueTable(recs.Select(x => (ITAG)x));
        public static Dictionary<string, string> GetValueTable(IEnumerable<CONFIG_DATA_SEND> sends)
            => GetValueTable(sends.Select(x => (ITAG)x));
        public static Dictionary<string, string> GetValueTable(IEnumerable<ITAG> SendsOrReceives) {
            Dictionary<string, string> res = new Dictionary<string, string>();

            foreach (ITAG item in SendsOrReceives) {
                res.Add(item.Name, "");
            }
            if (!res.ContainsKey(DATA.IPOC)) res.Add(DATA.IPOC, "-1");

            return res;
        }

        public static XmlDocument CreateXmlConfig(CONFIG_DATA Config, IEnumerable<CONFIG_DATA_SEND> Sends, IEnumerable<CONFIG_DATA_RECEIVE> Receives) {
            XmlDocument doc = new XmlDocument();
            XmlNode? rootNode = doc.AppendChild(doc.CreateElement("ROOT")) ?? throw new Exception("NO ROOT");

            // CONFIG
            XmlNode configsNode = doc.CreateElement("CONFIG");
            FieldInfo[] fields = typeof(CONFIG_DATA).GetFields();
            foreach (FieldInfo field in fields) {
                XmlNode fieldNode = doc.CreateElement(field.Name);
                object? value = field.GetValue(Config);
                string? valuestr;
                if (value != null && (valuestr = value.ToString()) != null)
                    fieldNode.InnerText = valuestr;
                configsNode.AppendChild(fieldNode);
            }
            rootNode.AppendChild(configsNode);

            // SENDS
            XmlNode sendNode = doc.CreateElement("SEND");
            XmlNode sendElementsNode = doc.CreateElement("ELEMENTS");
            PropertyInfo[] sendProps = typeof(CONFIG_DATA_SEND).GetProperties();
            foreach (CONFIG_DATA_SEND send in Sends) {
                XmlNode fieldNode = doc.CreateElement("ELEMENT");
                foreach (PropertyInfo prop in sendProps) {
                    XmlAttribute attribute = doc.CreateAttribute(prop.Name);
                    object? value = prop.GetValue(send);
                    if (value != null)
                        attribute.Value = value.ToString();

                    if (fieldNode.Attributes == null) throw new Exception("NULL ATRIBUTES");
                    fieldNode.Attributes.Append(attribute);
                }
                sendElementsNode.AppendChild(fieldNode);
            }
            sendNode.AppendChild(sendElementsNode);
            rootNode.AppendChild(sendNode);

            // RECEIVE
            XmlNode recNode = doc.CreateElement("RECEIVE");
            XmlNode recElementsNode = doc.CreateElement("ELEMENTS");

            PropertyInfo[] recProps = typeof(CONFIG_DATA_RECEIVE).GetProperties();
            foreach (CONFIG_DATA_RECEIVE rec in Receives) {
                XmlNode fieldNode = doc.CreateElement("ELEMENT");
                foreach (PropertyInfo prop in recProps) {
                    XmlAttribute attribute = doc.CreateAttribute(prop.Name);
                    object? value = prop.GetValue(rec);
                    if (value != null)
                        attribute.Value = value is bool bval ? (bval ? "1" : "0") : value.ToString();

                    if (fieldNode.Attributes == null) throw new Exception("NULL ATRIBUTES");
                    fieldNode.Attributes.Append(attribute);
                }
                recElementsNode.AppendChild(fieldNode);
            }
            recNode.AppendChild(recElementsNode);
            rootNode.AppendChild(recNode);


            return doc;
        }

        public static COMMON_CONFIG ParseConfigFromXmlConfig(XmlDocument xml) {
            COMMON_CONFIG commonConfig = new COMMON_CONFIG();

            // Parse CONFIG
            XmlNode? configNode = xml.SelectSingleNode("/ROOT/CONFIG") ?? throw new Exception("CONFIG node not found");
            commonConfig.CONFIG = new CONFIG_DATA();

            foreach (XmlNode fieldNode in configNode.ChildNodes) {
                switch (fieldNode.Name) {
                    case "IP_NUMBER":
                    commonConfig.CONFIG.IP_NUMBER = IPAddress.Parse(fieldNode.InnerText);
                    break;
                    case "PORT":
                    commonConfig.CONFIG.PORT = ushort.Parse(fieldNode.InnerText);
                    break;
                    case "SENTYPE":
                    commonConfig.CONFIG.SENTYPE = fieldNode.InnerText;
                    break;
                    case "ONLYSEND":
                    commonConfig.CONFIG.ONLYSEND = bool.Parse(fieldNode.InnerText);
                    break;
                }
            }

            // Parse SEND
            XmlNode? sendNode = xml.SelectSingleNode("/ROOT/SEND/ELEMENTS");
            commonConfig.SEND = new List<CONFIG_DATA_SEND>();

            if (sendNode != null) {
                foreach (XmlNode elementNode in sendNode.ChildNodes) {
                    CONFIG_DATA_SEND sendData = new CONFIG_DATA_SEND();
                    if (elementNode.Attributes != null)
                        foreach (XmlAttribute attribute in elementNode.Attributes) {
                            switch (attribute.Name) {
                                case "TAG":
                                sendData.Name = attribute.Value;
                                break;
                                case "TYPE":
                                sendData.ValueType = (TAG_VALUE_TYPE)Enum.Parse(typeof(TAG_VALUE_TYPE), attribute.Value);
                                break;
                                case "INDX":
                                sendData.INDX = attribute.Value;
                                break;
                            }
                        }

                    commonConfig.SEND.Add(sendData);
                }
            }

            // Parse RECEIVE
            XmlNode? recNode = xml.SelectSingleNode("/ROOT/RECEIVE/ELEMENTS");
            commonConfig.RECEIVE = new List<CONFIG_DATA_RECEIVE>();

            if (recNode != null) {
                foreach (XmlNode elementNode in recNode.ChildNodes) {
                    CONFIG_DATA_RECEIVE recData = new CONFIG_DATA_RECEIVE();
                    if (elementNode.Attributes != null)
                        foreach (XmlAttribute attribute in elementNode.Attributes) {
                            switch (attribute.Name) {
                                case "TAG":
                                recData.Name = attribute.Value;
                                break;
                                case "TYPE":
                                recData.ValueType = (TAG_VALUE_TYPE)Enum.Parse(typeof(TAG_VALUE_TYPE), attribute.Value);
                                break;
                                case "INDX":
                                recData.INDX = attribute.Value;
                                break;
                                case "HOLDON":
                                recData.HOLDON = attribute.Value == "1"; // Assuming "1" means true
                                break;
                            }
                        }

                    commonConfig.RECEIVE.Add(recData);
                }
            }

            return commonConfig;
        }

    }
}
