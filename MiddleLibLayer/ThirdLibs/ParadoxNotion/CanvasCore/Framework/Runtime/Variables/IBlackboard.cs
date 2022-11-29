using System.Collections.Generic;

namespace NodeCanvas.Framework
{

    ///<summary> An interface for Blackboards</summary>
    public interface IBlackboard
    {
        event System.Action<Variable> onVariableAdded;
        event System.Action<Variable> onVariableRemoved;

        string identifier { get; }
        IBlackboard parent { get; }
        Dictionary<string, Variable> variables { get; set; }
        UnityEngine.Component propertiesBindTarget { get; }
        UnityEngine.Object unityContextObject { get; }
        string independantVariablesFieldName { get; }

        void TryInvokeOnVariableAdded(Variable variable);
        void TryInvokeOnVariableRemoved(Variable variable);
    }

    ///<summary> An interface for Global Blackboards</summary>
    public interface IGlobalBlackboard : IBlackboard
    {
        string UID { get; }
    }
}