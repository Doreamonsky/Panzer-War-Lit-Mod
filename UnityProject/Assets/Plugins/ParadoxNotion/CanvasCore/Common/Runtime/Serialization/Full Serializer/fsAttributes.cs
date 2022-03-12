using System;

namespace ParadoxNotion.Serialization.FullSerializer
{
    ///<summary> Will make the field deserialize-only</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class fsWriteOnlyAttribute : Attribute { }

    ///<summary> Will make the field serialize-only</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class fsReadOnlyAttribute : Attribute { }

    ///<summary> Explicitly ignore a field from being serialized completely</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class fsIgnoreAttribute : Attribute { }

    ///<summary> Explicitly ignore a field from being serialized/deserialized in build</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class fsIgnoreInBuildAttribute : Attribute { }

    ///<summary> Explicitly opt in a field to be serialized and with specified name</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class fsSerializeAsAttribute : Attribute
    {
        readonly public string Name;
        public fsSerializeAsAttribute() { }
        public fsSerializeAsAttribute(string name) {
            this.Name = name;
        }
    }

    ///----------------------------------------------------------------------------------------------

    ///<summary> Use on a class to deserialize migrate into target type. This works in pair with IMigratable interface.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class fsMigrateToAttribute : System.Attribute
    {
        public readonly System.Type targetType;
        public fsMigrateToAttribute(System.Type targetType) {
            this.targetType = targetType;
        }
    }

    ///<summary> Use on a class to specify previous serialization versions to migrate from. This works in pair with IMigratable interface.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class fsMigrateVersionsAttribute : System.Attribute
    {
        public readonly System.Type[] previousTypes;
        public fsMigrateVersionsAttribute(params System.Type[] previousTypes) {
            this.previousTypes = previousTypes;
        }
    }

    ///<summary> Use on a class and field to request cycle references support</summary>
    // TODO: Refactor FS to only be required on field.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
    public sealed class fsSerializeAsReference : Attribute { }

    ///<summary> Use on a class to request try deserialize overwrite</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class fsDeserializeOverwrite : Attribute { }

    ///<summary> Use on a class to mark it for creating instance unititialized (which is faster)</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class fsUninitialized : System.Attribute { }

    ///<summary> Use on a class to request try create instance automatically on serialization</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class fsAutoInstance : Attribute
    {
        public readonly bool makeInstance;
        public fsAutoInstance(bool makeInstance = true) {
            this.makeInstance = makeInstance;
        }
    }

    ///<summary> This attribute controls some serialization behavior for a type. See the comments on each of the fields for more information.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class fsObjectAttribute : Attribute
    {
        //Converter override to use
        public Type Converter;
        //Processor to use
        public Type Processor;
    }

}