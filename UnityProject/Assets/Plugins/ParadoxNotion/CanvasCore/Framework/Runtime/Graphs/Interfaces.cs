using System.Collections.Generic;

namespace NodeCanvas.Framework
{

    ///Basically Nodes and Connections
    public interface IGraphElement
    {
        string name { get; }
        string UID { get; }
        Graph graph { get; }
        Status status { get; }
    }

    ///An interface to update nodes that need concurent updating apart from their normal 'Execution'.
    public interface IUpdatable : IGraphElement
    {
        void Update();
    }

    ///Denotes that the node can be invoked in means outside of it's 'Execution' scope.
    public interface IInvokable : IGraphElement
    {
        string GetInvocationID();
        object Invoke(params object[] args);
        void InvokeAsync(System.Action<object> callback, params object[] args);
    }

    ///Denotes that the node holds a nested graph.
    public interface IGraphAssignable : IGraphElement
    {
        Graph subGraph { get; set; }
        Graph currentInstance { get; set; }
        Dictionary<Graph, Graph> instances { get; set; }
        BBParameter subGraphParameter { get; }
        List<Internal.BBMappingParameter> variablesMap { get; set; }
    }

    ///Denotes that the node holds a nested graph of type T
    public interface IGraphAssignable<T> : IGraphAssignable where T : Graph
    {
        new T subGraph { get; set; }
        new T currentInstance { get; set; }
    }

    ///Denotes that the node can be assigned a Task and it's functionality is based on that task.
    public interface ITaskAssignable : IGraphElement
    {
        Task task { get; set; }
    }

    ///Use the generic ITaskAssignable when the Task type is known
    public interface ITaskAssignable<T> : ITaskAssignable where T : Task { }

    ///Just a simple way to have a link draw to target reference if any for nodes that do have a node reference
    public interface IHaveNodeReference : IGraphElement
    {
        INodeReference targetReference { get; }
    }

    ///Interface to handle reflection based wrappers
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