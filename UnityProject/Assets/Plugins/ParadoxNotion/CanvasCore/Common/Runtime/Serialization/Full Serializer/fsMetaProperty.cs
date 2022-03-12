using System;
using System.Reflection;

namespace ParadoxNotion.Serialization.FullSerializer
{
    ///<summary> A field on a MetaType.</summary>
    public class fsMetaProperty
    {

        ///<summary> Internal handle to the reflected member.</summary>
        public FieldInfo Field { get; private set; }
        ///<summary> The serialized name of the property, as it should appear in JSON.</summary>
        public string JsonName { get; private set; }
        ///<summary> The type of value that is stored inside of the property.</summary>
        public Type StorageType { get { return Field.FieldType; } }
        ///<summary> The real name of the member info.</summary>
        public string MemberName { get { return Field.Name; } }
        ///<summary> Is the property read only?</summary>
        public bool ReadOnly { get; private set; }
        ///<summary> Is the property write only?</summary>
        public bool WriteOnly { get; private set; }
        ///<summary> Make instance automatically?</summary>
        public bool AutoInstance { get; private set; }
        ///<summary> Serialize as reference?</summary>
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

        ///<summary> Reads a value from the property that this MetaProperty represents, using the given object instance as the context.</summary>
        public object Read(object context) {
            return Field.GetValue(context);
        }

        ///<summary> Writes a value to the property that this MetaProperty represents, using given object instance as the context.</summary>
        public void Write(object context, object value) {
            Field.SetValue(context, value);
        }
    }
}