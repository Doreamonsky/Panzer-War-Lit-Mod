using System.Collections.Generic;

namespace NodeCanvas.Framework
{

    ///<summary>Basically Nodes and Connections</summary>
    public interface IGraphElement
    {
        string name { get; }
        string UID { get; }
        Graph graph { get; }
        Status status { get; }
    }

    ///<summary>An interface to update nodes that need concurent updating apart from their normal 'Execution'.</summary>
    public interface IUpdatable : IGraphElement
    {
        void Update();
    }

    ///<summary>Denotes that the node can be invoked in means outside of it's 'Execution' scope.</summary>
    public interface IInvokable : IGraphElement
    {
        string GetInvocationID();
        object Invoke(params object[] args);
        void InvokeAsync(System.Action<object> callback, params object[] args);
    }

    ///<summary>Denotes that the node holds a nested graph.</summary>
    public interface IGraphAssignable : IGraphElement
    {
        Graph subGraph { get; set; }
        Graph currentInstance { get; set; }
        Dictionary<Graph, Graph> instances { get; set; }
        BBParameter subGraphParameter { get; }
        List<Internal.BBMappingParameter> variablesMap { get; set; }
    }

    ///<summary>Denotes that the node holds a nested graph of type T</summary>
    public interface IGraphAssignable<T> : IGraphAssignable where T : Graph
    {
        new T subGraph { get; set; }
        new T currentInstance { get; set; }
    }

    ///<summary>Denotes that the node can be assigned a Task and it's functionality is based on that task.</summary>
    public interface ITaskAssignable : IGraphElement
    {
        Task task { get; set; }
    }

    ///<summary>Use the generic ITaskAssignable when the Task type is known</summary>
    public interface ITaskAssignable<T> : ITaskAssignable where T : Task { }

    ///<summary>Just a simple way to have a link draw to target reference if any for nodes that do have a node reference</summary>
    public interface IHaveNodeReference : IGraphElement
    {
        INodeReference targetReference { get; }
    }

    ///<summary>Interface to handle reflection based wrappers</summary>
    public interface IReflectedWrapper
    {
        ParadoxNotion.Serialization.ISerializedReflectedInfo GetSerializedInfo();
    }

    //----------------------------------------------------------------------------------------------
    [System.Obsolete("This is no longer used nor required")]
    public interface ISubTasksContainer { Task[] GetSubTasks(); }
    [System.Obsolete("This is no longer used nor required")]
    public interface ISubParametersContainer { BBParameter[] GetSubParameters(); }
    //----------------------------------------------------------------------------------------------
}