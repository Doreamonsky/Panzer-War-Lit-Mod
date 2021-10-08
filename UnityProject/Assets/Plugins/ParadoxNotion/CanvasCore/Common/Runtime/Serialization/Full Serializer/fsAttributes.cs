using System;

namespace ParadoxNotion.Serialization.FullSerializer
{
    /// Will make the field deserialize-only
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class fsWriteOnlyAttribute : Attribute { }

    /// Will make the field serialize-only
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class fsReadOnlyAttribute : Attribute { }

    /// Explicitly ignore a field from being serialized completely
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class fsIgnoreAttribute : Attribute { }

    /// Explicitly ignore a field from being serialized/deserialized in build
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class fsIgnoreInBuildAttribute : Attribute { }

    /// Explicitly opt in a field to be serialized and with specified name
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

    /// Use on a class to deserialize migrate into target type.
    /// This works in pair with IMigratable interface.
    [AttributeUsage(AttributeTargets.Class)]
    public class fsMigrateToAttribute : System.Attribute
    {
        public readonly System.Type targetType;
        public fsMigrateToAttribute(System.Type targetType) {
            this.targetType = targetType;
        }
    }

    /// Use on a class to specify previous serialization versions to migrate from.
    /// This works in pair with IMigratable interface.
    [AttributeUsage(AttributeTargets.Class)]
    public class fsMigrateVersionsAttribute : System.Attribute
    {
        public readonly System.Type[] previousTypes;
        public fsMigrateVersionsAttribute(params System.Type[] previousTypes) {
            this.previousTypes = previousTypes;
        }
    }

    /// Use on a class and field to request cycle references support
    // TODO: Refactor FS to only be required on field.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
    public sealed class fsSerializeAsReference : Attribute { }

    /// Use on a class to request try deserialize overwrite
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class fsDeserializeOverwrite : Attribute { }

    /// Use on a class to mark it for creating instance unititialized (which is faster)
    [AttributeUsage(AttributeTargets.Class)]
    public class fsUninitialized : System.Attribute { }

    /// Use on a class to request try create instance automatically on serialization
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class fsAutoInstance : Attribute
    {
        public readonly bool makeInstance;
        public fsAutoInstance(bool makeInstance = true) {
            this.makeInstance = makeInstance;
        }
    }

    /// This attribute controls some serialization behavior for a type. See the comments
    /// on each of the fields for more information.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class fsObjectAttribute : Attribute
    {
        ///Converter override to use
        public Type Converter;
        ///Processor to use
        public Type Processor;
    }

}