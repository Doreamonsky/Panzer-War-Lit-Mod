using System;

namespace NodeCanvas.Framework.Internal
{
    ///<summary>Used to map subgraph variables <-> parent variables (or direct value)</summary>
    [Serializable]
    public class BBMappingParameter : BBObjectParameter
    {
        [UnityEngine.SerializeField] private string _targetSubGraphVariableID;
        [UnityEngine.SerializeField] private bool _canRead;
        [UnityEngine.SerializeField] private bool _canWrite;

        public string targetSubGraphVariableID => _targetSubGraphVariableID;
        public bool canRead { get { return _canRead; } set { _canRead = value; } }
        public bool canWrite { get { return _canWrite; } set { _canWrite = value; } }

        public BBMappingParameter() : base() { }
        public BBMappingParameter(Variable subVariable) {
            _targetSubGraphVariableID = subVariable.ID;
            base.SetType(subVariable.varType);
        }
    }
}