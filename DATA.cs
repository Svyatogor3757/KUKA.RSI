using System.Text.RegularExpressions;
using System.Text;
using System.Xml;

namespace KUKA.RSI {
    /// <summary>
    /// Класс для работы с данными робота
    /// </summary>
    public static class DATA {
        public const string IPOC = "IPOC";

        /// <summary>
        /// Кодировка по умолчанию
        /// </summary>
        public static Encoding DefaultEncoding { get; set; } = Encoding.UTF8;
        /// <summary>
        /// Преобразует байты в XML-документ согласно кодировке.
        /// Так же форматирует в пригодный для преобразование вид с помощью FormatXmlData.
        /// </summary>
        /// <param name="bytes">Байты XML-документа</param>
        /// <param name="encoding">Кодировка</param>
        /// <returns>XML-документ или null, если не удалось преобразовать</returns>
        public static XmlDocument? ParseXmlData(byte[]? bytes, Encoding? encoding = null) {
            encoding ??= DefaultEncoding;
            if (bytes == null || bytes.Length == 0) return null;
            return ParseXmlData(encoding.GetString(bytes));
        }
        /// <summary>
        /// Преобразует XML-строку в XML-документ правильного формата.
        /// Так же форматирует в пригодный для преобразование вид с помощью FormatXmlData.
        /// </summary>
        /// <param name="xml">XML-строка</param>
        public static XmlDocument? ParseXmlData(string xml) {
            try {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(FormatXmlData(xml));
                return xmlDoc;
            } catch {
                return null;
            }
        }
        /// <summary>
        /// Преобразует XML-строку в пригодный для преобразование вид. 
        /// Так как атрибуты не могут начинаться с цифры, то в начало добавляется символ "_". 
        /// Поэтому перед преобразованием отправкой на робот, необходимо вызвать unFormatXmlData. 
        /// </summary>
        public static string FormatXmlData(string xmlString) {
            string resXML = xmlString;
            int resupdCount = 0;
            // Шаг 1: Найти все элементы с аргументами
            string pattern = @"<(\w+)([^>]*)>";
            MatchCollection elements = Regex.Matches(xmlString, pattern);

            foreach (Match element in elements) {
                string eName = element.Groups[1].Value;
                string eAttribs = element.Groups[2].Value;

                // Шаг 2: Проверить аргументы на наличие цифр в начале
                string eUpdAttribs = eAttribs;
                int eupdCount = 0;
                MatchCollection keyValueMatches = Regex.Matches(eAttribs, @"(\w+)=(""[^""]*"")");
                foreach (Match kvelement in keyValueMatches) {
                    string key = kvelement.Groups[1].Value;
                    string value = kvelement.Groups[2].Value;
                    if (char.IsDigit(key[0])) {
                        int startIndex = kvelement.Index;
                        eUpdAttribs = eUpdAttribs.Insert(startIndex + eupdCount, "_");
                        eupdCount++;
                    }
                }

                // Шаг 3: Замена в оригинальном элементе
                // Если изменения были внесены, локализуем замену по позиции
                if (eUpdAttribs != eAttribs) {
                    int startIndex = element.Index;
                    resXML = resXML.Remove(startIndex + eName.Length + 1 + resupdCount, eAttribs.Length);
                    resXML = resXML.Insert(startIndex + eName.Length + 1 + resupdCount, eUpdAttribs);
                    if (eUpdAttribs.Length - eAttribs.Length < 0) throw new Exception("Error parse arg");
                    resupdCount += eUpdAttribs.Length - eAttribs.Length;
                }
            }
            return resXML;
        }
        /// <summary>
        /// Преобразует XML-документ в исходный вид
        /// Убирает знак _ в атрибутах после использования FormatXmlData
        /// </summary>
        public static string unFormatXmlData(string xmlString) {
            string resXML = xmlString;
            int resupdCount = 0;

            // Шаг 1: Найти все элементы с аргументами
            string pattern = @"<(\w+)([^>]*)>";
            MatchCollection elements = Regex.Matches(xmlString, pattern);

            foreach (Match element in elements) {
                string eName = element.Groups[1].Value;  // Element name
                string eAttribs = element.Groups[2].Value; // Element attributes

                // Шаг 2: Проверить аргументы на наличие цифр в начале
                string eUpdAttribs = eAttribs;
                int eupdCount = 0;

                MatchCollection keyValueMatches = Regex.Matches(eAttribs, @"(\w+)=(""[^""]*"")");
                foreach (Match kvelement in keyValueMatches) {
                    string key = kvelement.Groups[1].Value; // Get key
                    string value = kvelement.Groups[2].Value; // Get value
                    if (key.StartsWith("_")) {
                        // Remove the leading underscore
                        string newKey = key.Substring(1);
                        eUpdAttribs = eUpdAttribs.Replace($"{key}={value}", $"{newKey}={value}");
                        eupdCount++;
                    }
                }

                // Шаг 3: Замена в оригинальном элементе
                // Если изменения были внесены, локализуем замену по позиции
                if (eUpdAttribs != eAttribs) {
                    int startIndex = element.Index; // Position of the start of attributes
                    resXML = resXML.Remove(startIndex + eName.Length + 1 + resupdCount, eAttribs.Length);
                    resXML = resXML.Insert(startIndex + eName.Length + 1 + resupdCount, eUpdAttribs);

                    if (eUpdAttribs.Length - eAttribs.Length < 0) throw new Exception("Error parsing attribute");
                    resupdCount += eUpdAttribs.Length - eAttribs.Length;
                }
            }

            return resXML;
        }
        /// <summary>
        /// Форматирует XML-документ и переводит в строку
        /// </summary>
        public static string StringXmlFormat(XmlDocument doc) {
            StringBuilder sb = new StringBuilder();
            using (StringWriter stringWriter = new StringWriter(sb)) {
                using XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
                xmlTextWriter.Formatting = Formatting.Indented;
                doc.WriteTo(xmlTextWriter);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Преобразует XML-документ в словарь
        /// </summary>
        public static Dictionary<string, string> ConvertXmlToDict(XmlDocument data) {
            Dictionary<string, string> res = new Dictionary<string, string>();
            if (data.LastChild == null) return res;
            foreach (XmlNode item in data.LastChild.ChildNodes) {
                string key = item.Name;
                if (item.Attributes != null && item.Attributes.Count > 0) {
                    foreach (XmlAttribute itemAttr in item.Attributes) {
                        string akey = itemAttr.Name;
                        if (akey[0] == '_') akey = akey.Substring(1);
                        res.Add(key + "." + akey, itemAttr.Value.Trim('\"'));
                    }
                } else
                    res.Add(key, item.InnerText);
            }
            return res;
        }

        /// <summary>
        /// Преобразует словарь в XML-документ
        /// </summary>
        public static XmlDocument ConvertDictToXmlDoc(Dictionary<string, string>? ValueTable, string? IPOCForse = null, bool isSensor = true) {
            XmlDocument doc = new XmlDocument();
            /*  // Example
                <Sen Type="ImFree">
                <RKorr X="4" Y="7" Z="32" A="6" B="" C="6" />
                <AK A1="2" A2="54" A3="35" A4="76" A5="567" A6="785" />
                <EK E1="67" E2="67" E3="678" E4="3" E5="3" E6="7" />
                <DiO>123</DiO>
                <IPOC>123645634563</IPOC>
                </Sen>
            */
            XmlNode root = doc.CreateElement(isSensor ? "Sen" : "Rob");
            XmlAttribute rootAttrib = doc.CreateAttribute("Type");
            rootAttrib.InnerText = isSensor ? CONFIG.DefaultNameSensor : CONFIG.DefaultNameRobot;
            if (root.Attributes == null)
                throw new XmlException("Attributes is NULL");
            root.Attributes.Append(rootAttrib);
            doc.AppendChild(root);
            ValueTable ??= new Dictionary<string, string>();
            if (IPOCForse != null) {
                if (ValueTable.ContainsKey(IPOC))
                    ValueTable[IPOC] = IPOCForse;
                else
                    ValueTable.Add(IPOC, IPOCForse);
            }

            foreach (KeyValuePair<string, string> key in ValueTable) {
                string ekey = key.Key;
                string eattrkey = "";
                if (ekey.Contains('.')) {
                    ekey = key.Key.Substring(0, key.Key.IndexOf('.'));
                    eattrkey = key.Key.Substring(key.Key.IndexOf('.') + 1);
                }
                XmlNodeList outNodes = doc.GetElementsByTagName(ekey);
                XmlNode enode;
                if (outNodes.Count < 1) {
                    enode = doc.CreateElement(ekey);
                    root.AppendChild(enode);
                } else {
                    XmlNode? tmpnode = outNodes[0];
                    if (tmpnode == null) continue;
                    enode = tmpnode;
                }
                if (string.IsNullOrWhiteSpace(eattrkey))
                    enode.InnerText = key.Value;
                else {
                    //Надо позже исправлять аргументы, если там возможно начало с цифры
                    XmlAttribute eAttrib = doc.CreateAttribute(eattrkey);
                    eAttrib.InnerText = key.Value;
                    if (enode.Attributes == null) throw new Exception("Attributes is NULL");
                    enode.Attributes.Append(eAttrib);
                }
            }

            return doc;
        }
    }
}
