#if UNITY_EDITOR
namespace ParadoxNotion.Design
{
    ///<summary>Have some commonly stuff used across most inspectors and helper functions. Keep outside of Editor folder since many runtime classes use this in #if UNITY_EDITOR. This is a partial class. Different implementation provide different tools, so that everything is referenced from within one class.</summary>
    public static partial class EditorUtils { }
}
#endif