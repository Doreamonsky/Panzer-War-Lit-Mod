namespace ParadoxNotion.Serialization.FullSerializer
{

    // Global configuration options.
    public static class fsGlobalConfig
    {

        ///<summary> Serialize default values?</summary>
        public static bool SerializeDefaultValues = false;

        ///<summary> Should deserialization be case sensitive? If this is false and the JSON has multiple members with the same keys only separated by case, then this results in undefined behavior.</summary>
        public static bool IsCaseSensitive = false;

        ///<summary> The attributes that will force a field or property to *not* be serialized. Ignore attribute take predecence.</summary>
        public static System.Type[] IgnoreSerializeAttributes =
        {
            typeof(System.NonSerializedAttribute),
            typeof(fsIgnoreAttribute)
        };

        ///<summary> The attributes that will force a field or property to be serialized. Ignore attribute take predecence.</summary>
        public static System.Type[] SerializeAttributes =
        {
            typeof(UnityEngine.SerializeField),
            typeof(fsSerializeAsAttribute)
        };

        ///<summary> If not null, this string format will be used for DateTime instead of the default one.</summary>
        public static string CustomDateTimeFormatString = null;

        ///<summary> Int64 and UInt64 will be serialized and deserialized as string for compatibility</summary>
        public static bool Serialize64BitIntegerAsString = false;

        ///<summary> Enums are serialized using their names by default. Setting this to true will serialize them as integers instead.</summary>
        public static bool SerializeEnumsAsInteger = true;
    }
}