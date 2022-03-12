using System;
using System.Collections.Generic;
using UnityEngine;

namespace NodeCanvas.Framework.Internal
{

    ///<summary> Blackboard holds Variable and is able to save and load itself. It's usefull for interop communication within the program, saving and loading systems etc. This is the main implementation class of IBlackboard and the one being serialized.</summary>
    [Serializable]
    public class BlackboardSource : IBlackboard
    {

        [SerializeField] private Dictionary<string, Variable> _variables = new Dictionary<string, Variable>(StringComparer.Ordinal);

        public event System.Action<Variable> onVariableAdded;
        public event System.Action<Variable> onVariableRemoved;

        public string identifier => "Graph";
        public Dictionary<string, Variable> variables { get { return _variables; } set { _variables = value; } }
        public IBlackboard parent { get; set; }
        public UnityEngine.Object unityContextObject { get; set; }
        public Component propertiesBindTarget { get; set; }
        string IBlackboard.independantVariablesFieldName => null;

        void IBlackboard.TryInvokeOnVariableAdded(Variable variable) { if ( onVariableAdded != null ) onVariableAdded(variable); }
        void IBlackboard.TryInvokeOnVariableRemoved(Variable variable) { if ( onVariableRemoved != null ) onVariableRemoved(variable); }

        //required
        public BlackboardSource() { }

        public override string ToString() { return identifier; }
    }
}