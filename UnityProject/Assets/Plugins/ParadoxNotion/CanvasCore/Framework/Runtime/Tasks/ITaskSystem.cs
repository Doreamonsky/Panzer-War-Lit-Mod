using UnityEngine;

namespace NodeCanvas.Framework
{
    ///An interface used to provide default agent and blackboard references to tasks and let tasks 'interface' with the root system
    public interface ITaskSystem
    {
        Component agent { get; }
        IBlackboard blackboard { get; }
        Object contextObject { get; }
        float elapsedTime { get; }
        float deltaTime { get; }
        void UpdateTasksOwner();
        void SendEvent(string name, object value, object sender);
        void SendEvent<T>(string name, T value, object sender);
    }
}