using System.Net;
using System.Net.Sockets;
using System.Xml;

namespace KUKA.RSI {

    /// <summary>
    /// Класс для работы и общения с KUKA роботом по RSI протоколу.
    /// Данный класс позволяет создать RSI Sensor и обмениваться с роботом XML-документами по RSI протоколу.
    /// Данный класс реализует базовые несвязанные методы чтения и записи тегов робота.
    /// Рекомендуется использовать его в качестве базового класса или создать оболочку для управлением более высокого уровня.
    /// Так как необходимо отправлять данные сразу после получения с разницой в 4 мс. 
    /// При несоблюдении диапазона произойдет разъединение со стороны робота.
    /// </summary>
    public class RSIClient : IEquatable<RSIClient?>, IDisposable {
        /// <summary>
        /// Полученные данные от робота.
        /// </summary>
        public List<RSIData> Data { get; protected set; }
        /// <summary>
        /// Порт для приема данных с робота.
        /// </summary>
        public uint SensorPort { get; init; }
        /// <summary>
        /// UDP Клиент для обмена данными с роботом.
        /// </summary>
        public UdpClient Client { get; set; }
        /// <summary>
        /// IP адрес и порт робота.
        /// </summary>
        public IPEndPoint RobotEndPoint { get => robotEndPoint; set => robotEndPoint = value; }
        /// <summary>
        /// Таймаут приёма udp пакета
        /// </summary>
        public int ReceiveTimeout { get => Client.Client.ReceiveTimeout; set => Client.Client.ReceiveTimeout = value; }
        /// <summary>
        /// Таймаут отправки udp пакета
        /// </summary>
        public int SendTimeout { get => Client.Client.SendTimeout; set => Client.Client.SendTimeout = value; }

        protected IPEndPoint robotEndPoint;
        protected IPEndPoint sensorEndPoint;
        /// <summary>
        /// Токен отмены асинхронной операции.
        /// </summary>
        protected CancellationTokenSource cts = new CancellationTokenSource();
        /// <summary>
        /// Пустой IPEndPoint
        /// </summary>
        protected static readonly IPEndPoint EmptyIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
        /// <summary>
        /// Создание RSIClient'а с указанием порта для приема данных.
        /// </summary>
        /// <param name="sensorport">Порт приёма данных</param>
        public RSIClient(uint sensorport) {
            Data = new();
            SensorPort = sensorport;
            Client = new UdpClient();
            Client.ReInitPort((int)SensorPort);
            sensorEndPoint = new IPEndPoint(IPAddress.Any, (int)SensorPort);
            robotEndPoint = EmptyIPEndPoint;
            SendTimeout = ReceiveTimeout = 5;
        }

        public RSIClient(IPEndPoint iPEndPoint) {
            Data = new();
            SensorPort = (uint)iPEndPoint.Port;
            Client = new UdpClient();
            Client.Client.Bind(iPEndPoint);
            sensorEndPoint = iPEndPoint;
            robotEndPoint = EmptyIPEndPoint;
            SendTimeout = ReceiveTimeout = 5;

        }

        public RSIClient(string IPAndPort) : this(IPEndPoint.Parse(IPAndPort)) {

        }
        public RSIClient(string ip, uint port) : this(new IPEndPoint(IPAddress.Parse(ip), (int)port)) {

        }


        /// <summary>
        /// Получение данных от робота.
        /// </summary>
        /// <returns>True, если данные приняты и интерпретированы успешно</returns>
        public bool GetData() {
            XmlDocument? doc = null;
            DateTime dtCurrent = DateTime.Now;

            byte[]? receivedBytes = Client.Receive(ref robotEndPoint);
            if (receivedBytes != null)
                doc = DATA.ParseXmlData(receivedBytes);

            if (doc == null)
                return false;

            RSIData data = new RSIData(dtCurrent) {
                Source = doc,
                Tags = DATA.ConvertXmlToDict(doc)
            };
            Data.Add(data);
            return true;
        }

        /// <summary>
        /// Асинхронное получение данных от робота.
        /// </summary>
        /// <returns>True, если данные приняты и интерпретированы успешно</returns>
        /// <exception cref="System.OperationCanceledException"></exception>
        public async Task<bool> GetDataAsync() {
            XmlDocument? doc = null;
            DateTime dtCurrent = DateTime.Now;

            UdpReceiveResult result = await Client.ReceiveAsync(cts.Token);
            if (result.Buffer != null)
                doc = DATA.ParseXmlData(result.Buffer);
            RobotEndPoint = result.RemoteEndPoint;
            if (doc == null)
                return false;

            Dictionary<string, string> datatags = await Task.Run(() => DATA.ConvertXmlToDict(doc));
            RSIData data = new RSIData(dtCurrent) {
                Source = doc,
                Tags = datatags
            };
            Data.Add(data);
            return true;
        }
        /// <summary>
        /// Отправка таблицы ключ-значение на робот.
        /// </summary>
        /// <param name="ValueTable">Таблица ключ-значение</param>
        /// <returns>True, если данные успешно отправлены. 
        /// Учтите, что UDP соединение не поддерживает обратную связь. 
        /// Поэтому нет гарантии, что данные придут на робот.</returns>
        protected bool SendTags(Dictionary<string, string> ValueTable) {
            return SendStringData(DATA.ConvertDictToXmlDoc(ValueTable));
        }
        /// <summary>
        /// Отправка XML строки на робот.
        /// </summary>
        /// <param name="xmlString">XML строка</param>
        /// <returns>True, если данные успешно отправлены. 
        /// Учтите, что UDP соединение не поддерживает обратную связь. 
        /// Поэтому нет гарантии, что данные придут на робот.</returns>
        public bool SendStringData(string xmlString) {
            if (RobotEndPoint == null || RobotEndPoint == EmptyIPEndPoint)
                return false;
            byte[] data = DATA.DefaultEncoding.GetBytes(xmlString);
            int sended = Client.Send(data, data.Length, RobotEndPoint);
            return sended == data.Length;
        }
        /// <summary>
        /// Отправка XML документа на робот.
        /// </summary>
        /// <param name="xmlString">XML документ</param>
        /// <returns>True, если данные успешно отправлены. 
        /// Учтите, что UDP соединение не поддерживает обратную связь. 
        /// Поэтому нет гарантии, что данные придут на робот.</returns>
        public bool SendStringData(XmlDocument xmlString) {
            return SendStringData(xmlString.OuterXml);
        }
        /// <summary>
        /// Очистка принятых данных
        /// </summary>
        /// <param name="limitcount">Предел данных, когда необходимо их очищать</param>
        /// <param name="savecount">Количество сохраняемых данных, после очистки</param>
        /// <exception cref="ArgumentException"></exception>
        public void ClearData(int limitcount = 1000, int savecount = 100) {
            if (savecount > limitcount)
                throw new ArgumentException("Save count must be less than limit count");
            if (Data.Count > limitcount && Data.Count > savecount && Data.Count - savecount > 0)
                Data.RemoveRange(0, Data.Count - savecount);
        }
        /// <summary>
        /// Отмена асинхронной операции чтения данных.
        /// </summary>
        public void CancelAsync() {
            cts.Cancel();
        }

        public override bool Equals(object? obj) {
            return Equals(obj as RSIClient);
        }

        public bool Equals(RSIClient? other) {
            return other is not null &&
                   EqualityComparer<List<RSIData>>.Default.Equals(Data, other.Data) &&
                   SensorPort == other.SensorPort &&
                   EqualityComparer<IPEndPoint>.Default.Equals(robotEndPoint, other.robotEndPoint) &&
                   EqualityComparer<IPEndPoint>.Default.Equals(sensorEndPoint, other.sensorEndPoint);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Data, SensorPort, robotEndPoint, sensorEndPoint);
        }

        public static bool operator ==(RSIClient? left, RSIClient? right) {
            return EqualityComparer<RSIClient>.Default.Equals(left, right);
        }

        public static bool operator !=(RSIClient? left, RSIClient? right) {
            return !(left == right);
        }

        public void Dispose() {
            if (!cts.IsCancellationRequested)
                cts.Cancel();
            Client.Dispose();
            Data.Clear();
        }

        public override string? ToString() {
            return $"Client {sensorEndPoint} received the following data from server " +
                   $"{robotEndPoint}: {string.Join(",", Data.Select(x => x.ToString()))}";
        }
    }


}
