using System;

namespace ParadoxNotion.Design
{

    ///<summary>Marker attribute to include generic type or a type's generic methods in the AOT spoof generation</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Delegate)]
    public class SpoofAOTAttribute : Attribute { }

    ///<summary>To exclude a type from being listed. Abstract classes are not listed anyway.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DoNotListAttribute : Attribute { }

    ///<summary>When a type should for some reason be marked as protected so to always have one instance active.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ProtectedSingletonAttribute : Attribute { }

    ///<summary>Use for execution prioratizing when it matters.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExecutionPriorityAttribute : Attribute
    {
        readonly public int priority;
        public ExecutionPriorityAttribute(int priority) {
            this.priority = priority;
        }
    }

    ///<summary>Marks a generic type to be exposed at it's base definition rather than wrapping all preferred types around it.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExposeAsDefinitionAttribute : Attribute { }

    ///<summary>Marks a field to be exposed for inspection even if private (within the context of custom inspector).</summary>
    ///<summary>In custom inspector, private fields even if with [SerializedField] are not exposed by default.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ExposeFieldAttribute : Attribute { }

    ///<summary>Options attribute for list inspector editors</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ListInspectorOptionAttribute : Attribute
    {
        readonly public bool allowAdd;
        readonly public bool allowRemove;
        readonly public bool showFoldout;
        public ListInspectorOptionAttribute(bool allowAdd, bool allowRemove, bool alwaysExpanded) {
            this.allowAdd = allowAdd;
            this.allowRemove = allowRemove;
            this.showFoldout = alwaysExpanded;
        }
    }

    ///----------------------------------------------------------------------------------------------

    ///<summary>Use for friendly names and optional priority in relation to naming only</summary>
    [AttributeUsage(AttributeTargets.All)]
    public class NameAttribute : Attribute
    {
        readonly public string name;
        readonly public int priority;
        public NameAttribute(string name, int priority = 0) {
            this.name = name;
            this.priority = priority;
        }
    }

    ///<summary>Use for categorization</summary>
    [AttributeUsage(AttributeTargets.All)]
    public class CategoryAttribute : Attribute
    {
        readonly public string category;
        public CategoryAttribute(string category) {
            this.category = category;
        }
    }

    ///<summary>Use to give a description</summary>
    [AttributeUsage(AttributeTargets.All)]
    public class DescriptionAttribute : Attribute
    {
        readonly public string description;
        public DescriptionAttribute(string description) {
            this.description = description;
        }
    }

    ///<summary>When a type is associated with an icon</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class IconAttribute : Attribute
    {
        readonly public string iconName;
        readonly public bool fixedColor;
        readonly public string runtimeIconTypeCallback;
        readonly public Type fromType;
        public IconAttribute(string iconName = "", bool fixedColor = false, string runtimeIconTypeCallback = "") {
            this.iconName = iconName;
            this.fixedColor = fixedColor;
            this.runtimeIconTypeCallback = runtimeIconTypeCallback;
        }
        public IconAttribute(Type fromType) {
            this.fromType = fromType;
        }
    }

    ///<summary>When a type is associated with a color (provide in hex string without "#")</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ColorAttribute : Attribute
    {
        readonly public string hexColor;
        public ColorAttribute(string hexColor) {
            this.hexColor = hexColor;
        }
    }
}