using System;
using System.Reflection;

namespace ParadoxNotion.Serialization.FullSerializer
{
    /// A field on a MetaType.
    public class fsMetaProperty
    {

        /// Internal handle to the reflected member.
        public FieldInfo Field { get; private set; }
        /// The serialized name of the property, as it should appear in JSON.
        public string JsonName { get; private set; }
        /// The type of value that is stored inside of the property.
        public Type StorageType { get { return Field.FieldType; } }
        /// The real name of the member info.
        public string MemberName { get { return Field.Name; } }
        /// Is the property read only?
        public bool ReadOnly { get; private set; }
        /// Is the property write only?
        public bool WriteOnly { get; private set; }
        /// Make instance automatically?
        public bool AutoInstance { get; private set; }
        /// Serialize as reference?
        public bool AsReference { get; private set; }

        internal fsMetaProperty(FieldInfo field) {
            this.Field = field;
            var attr = Field.RTGetAttribute<fsSerializeAsAttribute>(true);
            this.JsonName = attr != null && !string.IsNullOrEmpty(attr.Name) ? attr.Name : field.Name;
            this.ReadOnly = Field.RTIsDefined<fsReadOnlyAttribute>(true);
            this.WriteOnly = Field.RTIsDefined<fsWriteOnlyAttribute>(true);
            var autoInstanceAtt = StorageType.RTGetAttribute<fsAutoInstance>(true);
            this.AutoInstance = autoInstanceAtt != null && autoInstanceAtt.makeInstance && !StorageType.IsAbstract;
            this.AsReference = Field.RTIsDefined<fsSerializeAsReference>(true);
        }

        /// Reads a value from the property that this MetaProperty represents, using the given
        /// object instance as the context.
        public object Read(object context) {
            return Field.GetValue(context);
        }

        /// Writes a value to the property that this MetaProperty represents, using given object
        /// instance as the context.
        public void Write(object context, object value) {
            Field.SetValue(context, value);
        }
    }
}