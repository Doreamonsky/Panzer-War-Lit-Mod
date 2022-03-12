using System.Collections.Generic;
using UnityEngine;
using ParadoxNotion;

namespace NodeCanvas.Framework
{
    ///<summary>A Signal definition that things can listen to. Can also be invoked in code by calling 'Invoke' but args have to be same type and same length as the parameters defined.</summary>
    [CreateAssetMenu(menuName = "ParadoxNotion/CanvasCore/Signal Definition")]
    public class SignalDefinition : ScriptableObject
    {

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void Editor_Init() {
            ParadoxNotion.Design.AssetTracker.BeginTrackingAssetsOfType(typeof(SignalDefinition));
        }
#endif

        public delegate void InvokeArguments(Transform sender, Transform receiver, bool isGlobal, params object[] args);
        public event InvokeArguments onInvoke;

        [SerializeField, HideInInspector]
        private List<DynamicParameterDefinition> _parameters = new List<DynamicParameterDefinition>();

        ///<summary>The Signal parameters</summary>
        public List<DynamicParameterDefinition> parameters {
            get { return _parameters; }
            private set { _parameters = value; }
        }

        ///<summary>Invoke the Signal</summary>
        public void Invoke(Transform sender, Transform receiver, bool isGlobal, params object[] args) {
            if ( onInvoke != null ) {
                onInvoke(sender, receiver, isGlobal, args);
            }
        }

        //...
        public void AddParameter(string name, System.Type type) {
            var param = new DynamicParameterDefinition(name, type);
            _parameters.Add(param);
        }

        //...
        public void RemoveParameter(string name) {
            var param = _parameters.Find(p => p.name == name);
            if ( param != null ) {
                _parameters.Remove(param);
            }
        }
    }
}