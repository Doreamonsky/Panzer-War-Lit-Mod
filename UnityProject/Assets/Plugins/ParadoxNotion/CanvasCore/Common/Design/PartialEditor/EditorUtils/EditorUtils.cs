#if UNITY_EDITOR
namespace ParadoxNotion.Design
{
    ///Have some commonly stuff used across most inspectors and helper functions. Keep outside of Editor folder since many runtime classes use this in #if UNITY_EDITOR
    ///This is a partial class. Different implementation provide different tools, so that everything is referenced from within one class.
    public static partial class EditorUtils { }
}
#endif