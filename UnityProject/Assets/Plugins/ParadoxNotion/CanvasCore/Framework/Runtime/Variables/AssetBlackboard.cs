using UnityEngine;
using System.Collections.Generic;
using NodeCanvas.Framework.Internal;
using ParadoxNotion.Serialization;

namespace NodeCanvas.Framework
{

    [CreateAssetMenu(menuName = "ParadoxNotion/CanvasCore/Blackboard Asset")]
    public class AssetBlackboard : ScriptableObject, ISerializationCallbackReceiver, IGlobalBlackboard
    {

        public event System.Action<Variable> onVariableAdded;
        public event System.Action<Variable> onVariableRemoved;

        [SerializeField] private string _serializedBlackboard;
        [SerializeField] private List<UnityEngine.Object> _objectReferences;
        [SerializeField] private string _UID = System.Guid.NewGuid().ToString();

        [System.NonSerialized] private string _identifier;
        [System.NonSerialized] private BlackboardSource _blackboard = new BlackboardSource();

        ///----------------------------------------------------------------------------------------------
        void ISerializationCallbackReceiver.OnBeforeSerialize() { SelfSerialize(); }
        void ISerializationCallbackReceiver.OnAfterDeserialize() { SelfDeserialize(); }
        ///----------------------------------------------------------------------------------------------

        void SelfSerialize() {
            _objectReferences = new List<UnityEngine.Object>();
            _serializedBlackboard = JSONSerializer.Serialize(typeof(BlackboardSource), _blackboard, _objectReferences);
        }

        void SelfDeserialize() {
            _blackboard = JSONSerializer.Deserialize<BlackboardSource>(_serializedBlackboard, _objectReferences);
            if ( _blackboard == null ) { _blackboard = new BlackboardSource(); }
        }

        ///----------------------------------------------------------------------------------------------

        Dictionary<string, Variable> IBlackboard.variables { get { return _blackboard.variables; } set { _blackboard.variables = value; } }
        UnityEngine.Object IBlackboard.unityContextObject => this;
        IBlackboard IBlackboard.parent => null;
        Component IBlackboard.propertiesBindTarget => null;
        string IBlackboard.independantVariablesFieldName => null;

        void IBlackboard.TryInvokeOnVariableAdded(Variable variable) { if ( onVariableAdded != null ) onVariableAdded(variable); }
        void IBlackboard.TryInvokeOnVariableRemoved(Variable variable) { if ( onVariableRemoved != null ) onVariableRemoved(variable); }

        public string identifier => _identifier;
        public string UID => _UID;

        [ContextMenu("Show Json")]
        void ShowJson() { JSONSerializer.ShowData(_serializedBlackboard, this.name); }

        public override string ToString() { return identifier; }
        void OnValidate() { _identifier = this.name; }

        ///----------------------------------------------------------------------------------------------

#if UNITY_EDITOR
        private string tempJson;
        private List<UnityEngine.Object> tempObjects;
        private bool bindingInit;

        //...
        void PlayModeChange(UnityEditor.PlayModeStateChange state) {
            if ( state == UnityEditor.PlayModeStateChange.EnteredPlayMode ) {
                tempJson = _serializedBlackboard;
                tempObjects = _objectReferences;
                if ( !bindingInit ) { this.InitializePropertiesBinding(null, false); }
            }
            if ( state == UnityEditor.PlayModeStateChange.ExitingPlayMode ) {
                _serializedBlackboard = tempJson;
                _objectReferences = tempObjects;
                bindingInit = false;
                SelfDeserialize();
            }
        }
#endif


        //...
        void OnEnable() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= PlayModeChange;
            UnityEditor.EditorApplication.playModeStateChanged += PlayModeChange;
#endif
            if ( ParadoxNotion.Services.Threader.applicationIsPlaying ) {
                this.InitializePropertiesBinding(null, false);
#if UNITY_EDITOR
                bindingInit = true;
#endif
            }

        }
    }
}