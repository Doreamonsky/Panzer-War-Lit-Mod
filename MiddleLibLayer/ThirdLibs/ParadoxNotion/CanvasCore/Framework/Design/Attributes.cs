using System;

namespace NodeCanvas.Framework
{
    //An attribute to help with URLs in the welcome window. Thats all.
    [AttributeUsage(AttributeTargets.Class)]
    public class GraphInfoAttribute : Attribute
    {
        public string packageName;
        public string docsURL;
        public string resourcesURL;
        public string forumsURL;
    }

    ///<summary>Used on top of IGraphAssignable nodes to specify the target type for DragDrop operations It can be used on top of other node types if the graph checks for that (see GraphEditorUtility.GetDropedReferenceNodeTypes)</summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class DropReferenceType : System.Attribute
    {
        public readonly System.Type type;
        public DropReferenceType(System.Type type) { this.type = type; }
    }

    ///<summary>Marks the BBParameter possible to only pick values from a blackboard.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class BlackboardOnlyAttribute : Attribute { }
}