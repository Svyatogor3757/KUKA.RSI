using KUKA.RSI;
using KUKA;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;


namespace Test1 {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void RSICreateConfig() {

            COMMON_CONFIG cconfig = CreateConfig();
            XmlDocument doc1 = CONFIG.CreateXmlConfig(cconfig.CONFIG, cconfig.SEND, cconfig.RECEIVE);
            XmlDocument doc2 = CONFIG.CreateXmlConfig(cconfig);

            XmlNode? configNode = doc1.SelectSingleNode("/ROOT/CONFIG/IP_NUMBER");
            Assert.IsNotNull(configNode);
            Assert.AreEqual(configNode.InnerText, cconfig.CONFIG.IP_NUMBER.ToString());

            XmlNodeList? nodes = doc1.SelectNodes("/ROOT/SEND/ELEMENTS/ELEMENT");
            Assert.IsNotNull(nodes);
            Assert.AreEqual(nodes.Count, cconfig.SEND.Count);
            foreach (XmlNode node in nodes) {
                Assert.IsNotNull(node.Attributes);
                XmlAttribute? nINDX = node.Attributes["INDX"];
                XmlAttribute? nTAG = node.Attributes["TAG"];
                XmlAttribute? nTYPE = node.Attributes["TYPE"];
                Assert.IsNotNull(nINDX);
                Assert.IsNotNull(nTAG);
                Assert.IsNotNull(nTYPE);
                Assert.IsFalse(string.IsNullOrWhiteSpace(nINDX.InnerText));
                Assert.IsFalse(string.IsNullOrWhiteSpace(nTAG.InnerText));
                Assert.IsFalse(string.IsNullOrWhiteSpace(nTYPE.InnerText));
                Assert.IsTrue(cconfig.SEND.Where(
                    x => x.INDX == nINDX.InnerText &&
                    x.Name == nTAG.InnerText &&
                    x.ValueType.ToString().ToUpper() == nTYPE.InnerText.ToUpper()).Count() > 0);
            }

            nodes = doc1.SelectNodes("/ROOT/RECEIVE/ELEMENTS/ELEMENT");
            Assert.IsNotNull(nodes);
            Assert.AreEqual(nodes.Count, cconfig.RECEIVE.Count);
            foreach (XmlNode node in nodes) {
                Assert.IsNotNull(node.Attributes);
                XmlAttribute? nINDX = node.Attributes["INDX"];
                XmlAttribute? nTAG = node.Attributes["TAG"];
                XmlAttribute? nTYPE = node.Attributes["TYPE"];
                XmlAttribute? nHOLDON = node.Attributes["HOLDON"];
                Assert.IsNotNull(nINDX);
                Assert.IsNotNull(nTAG);
                Assert.IsNotNull(nTYPE);
                Assert.IsNotNull(nHOLDON);
                Assert.IsFalse(string.IsNullOrWhiteSpace(nINDX.InnerText));
                Assert.IsFalse(string.IsNullOrWhiteSpace(nTAG.InnerText));
                Assert.IsFalse(string.IsNullOrWhiteSpace(nTYPE.InnerText));
                Assert.IsFalse(string.IsNullOrWhiteSpace(nHOLDON.InnerText));
                Assert.IsTrue(cconfig.RECEIVE.Where(
                    x => x.INDX == nINDX.InnerText &&
                    x.Name == nTAG.InnerText &&
                    x.ValueType.ToString().ToUpper() == nTYPE.InnerText.ToUpper() &&
                    x.HOLDON == (nHOLDON.InnerText == "1" ? true : false)).Count() > 0);
            }

            configNode = doc2.SelectSingleNode("/ROOT/CONFIG/IP_NUMBER");
            Assert.IsNotNull(configNode);
            Assert.AreEqual(configNode.InnerText, cconfig.CONFIG.IP_NUMBER.ToString());

            nodes = doc2.SelectNodes("/ROOT/SEND/ELEMENTS/ELEMENT");
            Assert.IsNotNull(nodes);
            Assert.AreEqual(nodes.Count, cconfig.SEND.Count);
            foreach (XmlNode node in nodes) {
                Assert.IsNotNull(node.Attributes);
                XmlAttribute? nINDX = node.Attributes["INDX"];
                XmlAttribute? nTAG = node.Attributes["TAG"];
                XmlAttribute? nTYPE = node.Attributes["TYPE"];
                Assert.IsNotNull(nINDX);
                Assert.IsNotNull(nTAG);
                Assert.IsNotNull(nTYPE);
                Assert.IsFalse(string.IsNullOrWhiteSpace(nINDX.InnerText));
                Assert.IsFalse(string.IsNullOrWhiteSpace(nTAG.InnerText));
                Assert.IsFalse(string.IsNullOrWhiteSpace(nTYPE.InnerText));
                Assert.IsTrue(cconfig.SEND.Where(
                    x => x.INDX == nINDX.InnerText &&
                    x.Name == nTAG.InnerText &&
                    x.ValueType.ToString().ToUpper() == nTYPE.InnerText.ToUpper()).Count() > 0);
            }

            nodes = doc2.SelectNodes("/ROOT/RECEIVE/ELEMENTS/ELEMENT");
            Assert.IsNotNull(nodes);
            Assert.AreEqual(nodes.Count, cconfig.RECEIVE.Count);
            foreach (XmlNode node in nodes) {
                Assert.IsNotNull(node.Attributes);
                XmlAttribute? nINDX = node.Attributes["INDX"];
                XmlAttribute? nTAG = node.Attributes["TAG"];
                XmlAttribute? nTYPE = node.Attributes["TYPE"];
                XmlAttribute? nHOLDON = node.Attributes["HOLDON"];
                Assert.IsNotNull(nINDX);
                Assert.IsNotNull(nTAG);
                Assert.IsNotNull(nTYPE);
                Assert.IsNotNull(nHOLDON);
                Assert.IsFalse(string.IsNullOrWhiteSpace(nINDX.InnerText));
                Assert.IsFalse(string.IsNullOrWhiteSpace(nTAG.InnerText));
                Assert.IsFalse(string.IsNullOrWhiteSpace(nTYPE.InnerText));
                Assert.IsFalse(string.IsNullOrWhiteSpace(nHOLDON.InnerText));
                Assert.IsTrue(cconfig.RECEIVE.Where(
                    x => x.INDX == nINDX.InnerText &&
                    x.Name == nTAG.InnerText &&
                    x.ValueType.ToString().ToUpper() == nTYPE.InnerText.ToUpper() &&
                    x.HOLDON == (nHOLDON.InnerText == "1" ? true : false)).Count() > 0);
            }



        }

        public static COMMON_CONFIG CreateConfig() {
            COMMON_CONFIG cconfig;
            CONFIG_DATA config = new CONFIG_DATA {
                PORT = 49152,
                IP_NUMBER = new IPAddress(new byte[] { 192, 168, 0, 1 }),
                ONLYSEND = false,
                SENTYPE = CONFIG.DefaultNameSensor
            };

            List<CONFIG_DATA_SEND> sends = new List<CONFIG_DATA_SEND>();
            int i = 1;
            sends.Add(new CONFIG_DATA_SEND() { Name = $"{DATA_TAG_TYPE.Out}.0{i}", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.BOOL });
            sends.Add(new CONFIG_DATA_SEND() { Name = $"{DATA_TAG_TYPE.Out}.0{i}", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.BOOL });
            sends.Add(new CONFIG_DATA_SEND() { Name = $"{DATA_TAG_TYPE.Out}.0{i}", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.BOOL });
            sends.Add(new CONFIG_DATA_SEND() { Name = $"{DATA_TAG_TYPE.Out}.0{i}", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.BOOL });
            sends.Add(new CONFIG_DATA_SEND() { Name = $"{DATA_TAG_TYPE.Out}.0{i}", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.BOOL });
            sends.Add(new CONFIG_DATA_SEND() { Name = $"{DATA_TAG_TYPE.FTC}.Fx", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.DOUBLE });
            sends.Add(new CONFIG_DATA_SEND() { Name = $"{DATA_TAG_TYPE.FTC}.Fy", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.DOUBLE });
            sends.Add(new CONFIG_DATA_SEND() { Name = $"{DATA_TAG_TYPE.FTC}.Fz", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.DOUBLE });
            sends.Add(new CONFIG_DATA_SEND() { Name = $"{DATA_TAG_TYPE.FTC}.Mx", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.DOUBLE });
            sends.Add(new CONFIG_DATA_SEND() { Name = $"{DATA_TAG_TYPE.FTC}.My", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.DOUBLE });
            sends.Add(new CONFIG_DATA_SEND() { Name = $"{DATA_TAG_TYPE.FTC}.Mz", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.DOUBLE });
            sends.Add(new CONFIG_DATA_SEND() { Name = $"{DATA_TAG_TYPE.Override}", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.LONG });

            List<CONFIG_DATA_RECEIVE> receives = new List<CONFIG_DATA_RECEIVE>();
            i = 1;
            receives.Add(new CONFIG_DATA_RECEIVE() { Name = $"{DATA_TAG_TYPE.RKorr}.X", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.DOUBLE, HOLDON = true });
            receives.Add(new CONFIG_DATA_RECEIVE() { Name = $"{DATA_TAG_TYPE.RKorr}.Y", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.DOUBLE, HOLDON = true });
            receives.Add(new CONFIG_DATA_RECEIVE() { Name = $"{DATA_TAG_TYPE.RKorr}.Z", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.DOUBLE, HOLDON = true });
            receives.Add(new CONFIG_DATA_RECEIVE() { Name = $"{DATA_TAG_TYPE.RKorr}.A", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.DOUBLE, HOLDON = true });
            receives.Add(new CONFIG_DATA_RECEIVE() { Name = $"{DATA_TAG_TYPE.RKorr}.B", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.DOUBLE, HOLDON = true });
            receives.Add(new CONFIG_DATA_RECEIVE() { Name = $"{DATA_TAG_TYPE.RKorr}.C", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.DOUBLE, HOLDON = true });
            receives.Add(new CONFIG_DATA_RECEIVE() { Name = $"{DATA_TAG_TYPE.DiO}", INDX = $"{i++}", ValueType = TAG_VALUE_TYPE.LONG, HOLDON = true });

            cconfig = new COMMON_CONFIG {
                SEND = sends,
                RECEIVE = receives,
                CONFIG = config
            };
            return cconfig;
        }

        [TestMethod]
        public void ParseConfigFromXmlConfig() {
            COMMON_CONFIG cconfig = CreateConfig();
            XmlDocument doc = CONFIG.CreateXmlConfig(cconfig);
            COMMON_CONFIG cconfigParse1 = CONFIG.ParseConfigFromXmlConfig(doc);
            COMMON_CONFIG cconfigParse2 = CONFIG.ParseConfigFromXmlConfig(doc.OuterXml);

            foreach (CONFIG_DATA_SEND send in cconfigParse1.SEND) {
                Assert.IsTrue(cconfig.SEND.Where(x => x.Equals(send)).Count() > 0);
            }
            foreach (CONFIG_DATA_RECEIVE rec in cconfigParse1.RECEIVE) {
                Assert.IsTrue(cconfig.RECEIVE.Where(x => x.Equals(rec)).Count() > 0);
            }
            Assert.IsTrue(cconfig.CONFIG.Equals(cconfigParse1.CONFIG));


            foreach (CONFIG_DATA_SEND send in cconfigParse2.SEND) {
                Assert.IsTrue(cconfig.SEND.Where(x => x.Equals(send)).Count() > 0);
            }
            foreach (CONFIG_DATA_RECEIVE rec in cconfigParse2.RECEIVE) {
                Assert.IsTrue(cconfig.RECEIVE.Where(x => x.Equals(rec)).Count() > 0);
            }

            Assert.IsTrue(cconfig.CONFIG.Equals(cconfigParse2.CONFIG));

        }
        /*
                [TestMethod]
                public void GetData() {
                    string hostName = "localhost";
                    ushort port = 5060;
                    RSISensor sernor = new RSISensor(port);
                    string xml = "<Rob TYPE=\"KUKA\">\r\n" +
                        "<Out 01=\"0\" 02=\"1\" 03=\"1\" 04=\"0\" 05=\"0\" />\r\n" +
                        "<FTC Fx=\"1.234\" Fy=\"54.75\" Fz=\"345.7\" Mx=\"2346.6\" My=\"12.0\" Mz=\"3546\" />\r\n" +
                        "<Override>90</Override>\r\n" +
                        "<IPOC>123645634563</IPOC>\r\n" +
                        "</Rob>";
                    XmlDocument? doc = DATA.ParseXmlData(xml);

                    Task.Run(() => {
                        bool get = sernor.GetData();
                        Assert.IsTrue(get);

                        Assert.IsTrue(sernor.Data.Last().Tags.ContainsKey("Out.01"));
                        Assert.IsTrue(sernor.Data.Last().Tags["Out.01"] == "0");
                        Assert.IsTrue(sernor.Data.Last().Tags.ContainsKey("IPOC"));
                        Assert.IsTrue(sernor.Data.Last().Tags["IPOC"] == "123645634563");
                    });

                    UdpClient udpClient = new UdpClient();

                    Assert.IsNotNull(doc);
                    string message = doc.OuterXml;
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    udpClient.Send(data, data.Length, hostName, port);
                    udpClient.Close();

                }

                [TestMethod]
                public void ParseValueTable() {
                    RSISensor sernor = new RSISensor();
                    string xml = "<Rob TYPE=\"KUKA\">\r\n" +
                        "<Out 01=\"0\" 02=\"1\" 03=\"1\" 04=\"0\" 05=\"0\" />\r\n" +
                        "<FTC Fx=\"1.234\" Fy=\"54.75\" Fz=\"345.7\" Mx=\"2346.6\" My=\"12.0\" Mz=\"3546\" />\r\n" +
                        "<Override>90</Override>\r\n" +
                        "<IPOC>123645634563</IPOC>\r\n" +
                        "</Rob>";
                    XmlDocument? doc = DATA.ParseXmlData(xml);
                    Assert.IsNotNull(doc);
                    Dictionary<string, string> tablevalues = DATA.ConvertXmlToDict(doc);

                    Assert.IsTrue(tablevalues.ContainsKey("Out.01"));
                    Assert.IsTrue(tablevalues["Out.01"] == "0");
                    Assert.IsTrue(tablevalues.ContainsKey("Out.02"));
                    Assert.IsTrue(tablevalues["Out.02"] == "1");
                    Assert.IsTrue(tablevalues.ContainsKey("Out.03"));
                    Assert.IsTrue(tablevalues["Out.03"] == "1");
                    Assert.IsTrue(tablevalues.ContainsKey("Out.04"));
                    Assert.IsTrue(tablevalues["Out.04"] == "0");
                    Assert.IsTrue(tablevalues.ContainsKey("Out.05"));
                    Assert.IsTrue(tablevalues["Out.05"] == "0");

                    Assert.IsTrue(tablevalues.ContainsKey("FTC.Fx"));
                    Assert.IsTrue(tablevalues["FTC.Fx"] == "1.234");
                    Assert.IsTrue(tablevalues.ContainsKey("FTC.Fy"));
                    Assert.IsTrue(tablevalues["FTC.Fy"] == "54.75");
                    Assert.IsTrue(tablevalues.ContainsKey("FTC.Fz"));
                    Assert.IsTrue(tablevalues["FTC.Fz"] == "345.7");

                    Assert.IsTrue(tablevalues.ContainsKey("FTC.Mx"));
                    Assert.IsTrue(tablevalues["FTC.Mx"] == "2346.6");
                    Assert.IsTrue(tablevalues.ContainsKey("FTC.My"));
                    Assert.IsTrue(tablevalues["FTC.My"] == "12.0");
                    Assert.IsTrue(tablevalues.ContainsKey("FTC.Mz"));
                    Assert.IsTrue(tablevalues["FTC.Mz"] == "3546");

                    Assert.IsTrue(tablevalues.ContainsKey("Override"));
                    Assert.IsTrue(tablevalues["Override"] == "90");

                    Assert.IsTrue(tablevalues.ContainsKey("IPOC"));
                    Assert.IsTrue(tablevalues["IPOC"] == "123645634563");

                }

                [TestMethod]
                public void PutData() {
                    string hostName = "localhost";
                    ushort port = 5060;
                    RSISensor sernor = new RSISensor(hostName, port);

                    Task.Run(() => {
                        bool getret = sernor.GetData();
                        Assert.IsTrue(getret);
                        Assert.IsTrue(sernor.Data.Last().Tags.ContainsKey("Out.01"));
                        Assert.IsTrue(sernor.Data.Last().Tags["Out.01"] == "0");
                        Assert.IsTrue(sernor.Data.Last().Tags.ContainsKey("IPOC"));
                        Assert.IsTrue(sernor.Data.Last().Tags["IPOC"] == "123645634563");
                    });
                    Dictionary<string, string> send = new Dictionary<string, string> {
                        { "Out.01", "0" },
                        { "IPOC", "123645634563" },
                        { "Other", "1234" }
                    };
                    bool putret = sernor.PutData(send);
                    Assert.IsTrue(putret);
                }*/



    }
}
