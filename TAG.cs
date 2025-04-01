namespace KUKA {

    public interface ITAG {
        public string Name { get; set; }
        public TAG_VALUE_TYPE ValueType { get; }

    }
    public enum TAG_VALUE_TYPE {
        NONE,
        INTERNAL,
        BOOL,
        REAL,
        DOUBLE,
        LONG,
        STRING,
        STREAM,
        BYTE,
        FRAME
    }

    public struct TAG : ITAG {
        public string Name { get; set; }
        public string Value { get; set; }
        public TAG_VALUE_TYPE ValueType { get; set; }

        public object? GetDefValue() {
            return GetDefValue(Value, ValueType);
        }

        public static object? GetDefValue(string VALUE, TAG_VALUE_TYPE TYPE) {
            switch (TYPE) {
                case TAG_VALUE_TYPE.NONE:
                default:
                return null;
                case TAG_VALUE_TYPE.STRING:
                return VALUE;
                case TAG_VALUE_TYPE.BOOL:
                if (bool.TryParse(VALUE, out bool val))
                    return val;
                else goto default;
                case TAG_VALUE_TYPE.DOUBLE:
                case TAG_VALUE_TYPE.REAL:
                if (double.TryParse(VALUE, out double val2))
                    return val2;
                else goto default;
                case TAG_VALUE_TYPE.LONG:
                if (int.TryParse(VALUE, out int val3))
                    return val3;
                else goto default;
            }
        }
    }
}

