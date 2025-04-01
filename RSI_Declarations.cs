using System.Net;


namespace KUKA.RSI {
    //public enum RSI_RET {
    //    RSIOK,
    //    RSIFILENOTFOUND,
    //    RSIINVFILE,
    //    RSINOMEMORY,
    //    RSIINVOBJTYPE,
    //    RSIEXTLIBNOTFOUND,
    //    RSINOTLINKED,
    //    RSILNKCIRCLE,
    //    RSIINVOBJID,
    //    RSIALREADYON,
    //    RSINOTRUNNING,
    //    RSIINVCONT,
    //    RSIINPARAMID,
    //    RSIINPARAM
    //}



    public enum DATA_TAG_TYPE {
        /// <summary xml:lang="ru">
        /// Фактическое положение в декартовой системе координат XYZABC
        /// </summary>
        DEF_RIst,
        /// <summary xml:lang="ru">
        /// Заданное положение в декартовой системе координат XYZABC
        /// </summary>
        DEF_RSol,
        /// <summary xml:lang="ru">
        /// Фактическое положение осей робота A1-A6
        /// </summary>
        DEF_AIPos, //A1-A6
        /// <summary xml:lang="ru">
        /// Заданное положение осей робота A1-A6
        /// </summary>
        DEF_ASPos, //A1-A6
        /// <summary xml:lang="ru">
        /// Фактическое положение дополнительных осей E1-E6
        /// </summary>
        DEF_EIPos, //E1-E6
        /// <summary xml:lang="ru">
        /// Заданное положение дополнительных осей E1-E6
        /// </summary>
        DEF_ESPos, //E1-E6
        /// <summary xml:lang="ru">
        /// Ток двигателей осей робота A1-A6
        /// </summary>
        DEF_MACur, //A1-A6
        /// <summary xml:lang="ru">
        /// Ток двигателей дополнительных осей E1-E6
        /// </summary>
        DEF_MECur,  //E1-E6
        /// <summary xml:lang="ru">
        /// Количество пакетов данных, поступивших с опозданием LONG
        /// </summary>
        DEF_Delay,
        /// <summary xml:lang="ru">
        /// Технологические параметры во время основного выполнения (C1-C6)
        /// и во время предварительного выполнения (T1-T6)
        /// </summary>
        DEF_Tech,

        Out,

        FTC,

        RKorr, //XYZABC

        AK, //A1-A6

        EK, //E1-E6

        DiO,
        /// <summary xml:lang="ru">
        /// Информационное сообщение или сообщение об ошибке
        /// </summary>
        EStr,
        DOSync,
        Calibration, //XYZABC
        ResultsReady,
        SetOvPro,
        IPOC,
        Override
    }

    public struct COMMON_CONFIG {
        public CONFIG_DATA CONFIG;
        public List<CONFIG_DATA_SEND> SEND;
        public List<CONFIG_DATA_RECEIVE> RECEIVE;
    }

    public struct CONFIG_DATA {
        public IPAddress IP_NUMBER;
        public ushort PORT;
        public string SENTYPE;
        public bool ONLYSEND;
    }


    public struct CONFIG_DATA_SEND : ITAG {
        public string Name { get; set; }
        public TAG_VALUE_TYPE ValueType { get; set; }
        public string INDX { get; set; } // С 1 отсчет

    }
    public struct CONFIG_DATA_RECEIVE : ITAG {
        public string Name { get; set; }
        public TAG_VALUE_TYPE ValueType { get; set; }
        public string INDX { get; set; }
        public bool HOLDON { get; set; }
    }


    public static class CONFIG_DATA_Exp {

        public static TAG_VALUE_TYPE GetRSIType(this ITAG obj, object value) {
            Type type = value.GetType();
            if (type == typeof(int))
                return TAG_VALUE_TYPE.LONG;
            if (type == typeof(float) || type == typeof(double))
                return TAG_VALUE_TYPE.REAL;
            if (type == typeof(byte))
                return TAG_VALUE_TYPE.BYTE;
            if (type == typeof(bool))
                return TAG_VALUE_TYPE.BOOL;
            if (type == typeof(long))
                return TAG_VALUE_TYPE.LONG;
            if (type == typeof(string))
                return TAG_VALUE_TYPE.STRING;
            throw new Exception("Failed to convert Type to KUKA_DATA_TYPE");
        }

        public static TAG_VALUE_TYPE GetRSIType(this ITAG obj, Type valuetype) {
            if (valuetype == typeof(int))
                return TAG_VALUE_TYPE.LONG;
            if (valuetype == typeof(float) || valuetype == typeof(double))
                return TAG_VALUE_TYPE.REAL;
            if (valuetype == typeof(byte))
                return TAG_VALUE_TYPE.BYTE;
            if (valuetype == typeof(bool))
                return TAG_VALUE_TYPE.BOOL;
            if (valuetype == typeof(long))
                return TAG_VALUE_TYPE.LONG;
            if (valuetype == typeof(string))
                return TAG_VALUE_TYPE.STRING;
            throw new Exception("Failed to convert Type to KUKA_DATA_TYPE");
        }
    }

}
