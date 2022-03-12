using System;

namespace ParadoxNotion.Design
{

    ///<summary>Derive this to create custom attributes to be drawn with an AttributeDrawer<T>.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    abstract public class DrawerAttribute : Attribute
    {
        virtual public int priority { get { return int.MaxValue; } }
        virtual public bool isDecorator { get { return false; } }
    }

    ///----------------------------------------------------------------------------------------------

    ///<summary>Will dim control for bool, int, float, string if its default value (or empty for string)</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class HeaderAttribute : DrawerAttribute
    {
        readonly public string title;
        public override bool isDecorator { get { return true; } }
        public HeaderAttribute(string title) {
            this.title = title;
        }
    }

    ///<summary>Will dim control for bool, int, float, string if its default value (or empty for string)</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class DimIfDefaultAttribute : DrawerAttribute
    {
        public override bool isDecorator { get { return true; } }
        public override int priority { get { return 0; } }
    }

    ///<summary>Use on top of any field to show it only if the provided field is equal to the provided check value</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ShowIfAttribute : DrawerAttribute
    {
        readonly public string fieldName;
        readonly public int checkValue;
        public override bool isDecorator { get { return true; } }
        public override int priority { get { return 1; } }
        public ShowIfAttribute(string fieldName, int checkValue) {
            this.fieldName = fieldName;
            this.checkValue = checkValue;
        }
    }

    ///<summary>Helper attribute. Denotes that the field is required not to be null or string.empty</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiredFieldAttribute : DrawerAttribute
    {
        public override bool isDecorator { get { return false; } }
        public override int priority { get { return 2; } }
    }

    ///<summary>Show a button above field</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ShowButtonAttribute : DrawerAttribute
    {
        readonly public string buttonTitle;
        readonly public string methodName;
        public override bool isDecorator { get { return true; } }
        public override int priority { get { return 3; } }
        public ShowButtonAttribute(string buttonTitle, string methodnameCallback) {
            this.buttonTitle = buttonTitle;
            this.methodName = methodnameCallback;
        }
    }

    ///<summary>Will invoke a callback method when the field is changed</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class CallbackAttribute : DrawerAttribute
    {
        readonly public string methodName;
        public override bool isDecorator { get { return true; } }
        public override int priority { get { return 4; } }
        public CallbackAttribute(string methodName) {
            this.methodName = methodName;
        }
    }

    ///----------------------------------------------------------------------------------------------

    ///<summary>Will clamp float or int value to min</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class MinValueAttribute : DrawerAttribute
    {
        public override int priority { get { return 5; } }
        readonly public float min;
        public MinValueAttribute(float min) {
            this.min = min;
        }
        public MinValueAttribute(int min) {
            this.min = min;
        }
    }

    ///----------------------------------------------------------------------------------------------

    ///<summary>Makes float, int or string field show in a delayed control</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class DelayedFieldAttribute : DrawerAttribute { }

    ///<summary>Makes the int field show as layerfield</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LayerFieldAttribute : DrawerAttribute { }

    ///<summary>Makes the string field show as tagfield</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class TagFieldAttribute : DrawerAttribute { }

    ///<summary>Makes the string field show as text field with specified number of lines</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class TextAreaFieldAttribute : DrawerAttribute
    {
        readonly public int numberOfLines;
        public TextAreaFieldAttribute(int numberOfLines) {
            this.numberOfLines = numberOfLines;
        }
    }

    ///<summary>Use on top of any type of field to restict values to the provided ones through a popup by providing a params array of options.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PopupFieldAttribute : DrawerAttribute
    {
        readonly public object[] options;
        public PopupFieldAttribute(params object[] options) {
            this.options = options;
        }
    }

    ///<summary>Makes the float or integer field show as slider</summary>
	[AttributeUsage(AttributeTargets.Field)]
    public class SliderFieldAttribute : DrawerAttribute
    {
        readonly public float min;
        readonly public float max;
        public SliderFieldAttribute(float min, float max) {
            this.min = min;
            this.max = max;
        }
        public SliderFieldAttribute(int min, int max) {
            this.min = min;
            this.max = max;
        }
    }

    ///<summary>Forces the field to show as a Unity Object field. Usefull for interface fields</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ForceObjectFieldAttribute : DrawerAttribute { }


}