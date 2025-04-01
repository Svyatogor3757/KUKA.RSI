using System.Xml;

namespace KUKA.RSI {
    /// <summary>
    /// Класс для хранения данных, полученных с робота
    /// </summary>
    public struct RSIData : IEquatable<RSIData> {
        /// <summary>
        /// Теги, полученные с робота
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }
        /// <summary>
        /// Исходный XML документ
        /// </summary>
        public XmlDocument Source { get; set; }
        /// <summary>
        /// Время получения данных
        /// </summary>
        public readonly DateTime ReceivingTime { get; init; }
        /// <summary>
        /// Пустой объект RSIData
        /// </summary>
        public static readonly RSIData Empty;

        static RSIData() {
            Empty = new RSIData();
        }
        /// <summary>
        /// Создать объект для хранения данных, полученных с робота
        /// </summary>
        /// <param name="reveive">Время получения данных</param>
        /// <param name="source">Исходный XML документ</param>
        /// <param name="tags">Таблица тегов</param>
        public RSIData(DateTime? reveive = null, XmlDocument? source = null, Dictionary<string, string>? tags = null) {
            ReceivingTime = reveive ?? DateTime.Now;
            Source = source ?? new XmlDocument();
            Tags = tags ?? new Dictionary<string, string>();

        }
        /// <summary>
        /// Время, пришедшее с робота (IPOC) в формате TimeSpan
        /// </summary>
        public readonly TimeSpan SendingTimeSpan {
            get {
                if (Tags != null && Tags.ContainsKey(DATA.IPOC))
                    return TimeSpan.FromMilliseconds(long.Parse(Tags[DATA.IPOC]));
                else
                    return TimeSpan.Zero;
            }
        }
        /// <summary>
        /// Время, пришедшее с робота (IPOC)
        /// </summary>
        /// <returns></returns>
        public readonly string GetIPOC() {
            if (Tags != null && Tags.ContainsKey(DATA.IPOC))
                return Tags[DATA.IPOC];
            else
                return "-1";
        }

        public override readonly string? ToString() {
            if (Tags == null) return null;
            return "[" + string.Join(",", Tags.Select(x => $"{x.Key}={x.Value}")) + "]";
        }

        public override readonly bool Equals(object? obj) {
            return obj is RSIData data && Equals(data);
        }

        public readonly bool Equals(RSIData other) {
            return EqualityComparer<Dictionary<string, string>>.Default.Equals(Tags, other.Tags) &&
                   EqualityComparer<XmlDocument>.Default.Equals(Source, other.Source) &&
                   ReceivingTime == other.ReceivingTime;
        }

        public override readonly int GetHashCode() {
            return HashCode.Combine(Tags, Source, ReceivingTime);
        }

        public static bool operator ==(RSIData left, RSIData right) {
            return left.Equals(right);
        }

        public static bool operator !=(RSIData left, RSIData right) {
            return !(left == right);
        }
    }
}
