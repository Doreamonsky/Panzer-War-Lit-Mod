namespace ParadoxNotion.Serialization.FullSerializer
{

    // Global configuration options.
    public static class fsGlobalConfig
    {

        /// Serialize default values?
        public static bool SerializeDefaultValues = false;

        /// Should deserialization be case sensitive? If this is false and the JSON has multiple members with the
        /// same keys only separated by case, then this results in undefined behavior.
        public static bool IsCaseSensitive = false;

        /// The attributes that will force a field or property to *not* be serialized.
        /// Ignore attribute take predecence.
        public static System.Type[] IgnoreSerializeAttributes =
        {
            typeof(System.NonSerializedAttribute),
            typeof(fsIgnoreAttribute)
        };

        /// The attributes that will force a field or property to be serialized.
        /// Ignore attribute take predecence.
        public static System.Type[] SerializeAttributes =
        {
            typeof(UnityEngine.SerializeField),
            typeof(fsSerializeAsAttribute)
        };

        /// If not null, this string format will be used for DateTime instead of the default one.
        public static string CustomDateTimeFormatString = null;

        /// Int64 and UInt64 will be serialized and deserialized as string for compatibility
        public static bool Serialize64BitIntegerAsString = false;

        /// Enums are serialized using their names by default. Setting this to true will serialize them as integers instead.
        public static bool SerializeEnumsAsInteger = true;
    }
}