using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization.FullSerializer;

namespace NodeCanvas.DialogueTrees
{

    [Name("Sub Dialogue Tree")]
    [Description("Execute a Sub Dialogue Tree. When that Dialogue Tree is finished, this node will continue either in Success or Failure if it has any connections. Useful for making reusable and self-contained Dialogue Trees.")]
    [DropReferenceType(typeof(DialogueTree))]
    public class SubDialogueTree : DTNodeNested<DialogueTree>, IUpdatable
    {

        [SerializeField, ExposeField]
        private BBParameter<DialogueTree> _subTree = null;
        [fsSerializeAs("actorParametersMap")]
        private Dictionary<string, string> _actorParametersMap = null;

        public override int maxOutConnections { get { return 2; } }

        public override DialogueTree subGraph { get { return _subTree.value; } set { _subTree.value = value; } }
        public override BBParameter subGraphParameter => _subTree;

        //

        protected override Status OnExecute(Component agent, IBlackboard bb) {

            if ( subGraph == null ) {
                return Error("No Sub Dialogue Tree assigned!");
            }

            currentInstance = (DialogueTree)this.CheckInstance();
            this.TryWriteAndBindMappedVariables();
            TryWriteMappedActorParameters();
            currentInstance.StartGraph(finalActor is Component ? (Component)finalActor : finalActor.transform, bb.parent, Graph.UpdateMode.Manual, OnSubDialogueFinish);
            return Status.Running;
        }

        void OnSubDialogueFinish(bool success) {
            this.TryReadAndUnbindMappedVariables();
            status = success ? Status.Success : Status.Failure;
            DLGTree.Continue(success ? 0 : 1);
        }

        void IUpdatable.Update() {
            if ( currentInstance != null && status == Status.Running ) {
                currentInstance.UpdateGraph(this.graph.deltaTime);
            }
        }

        void TryWriteMappedActorParameters() {
            if ( _actorParametersMap == null ) { return; }
            foreach ( var pair in _actorParametersMap ) {
                var targetParam = currentInstance.GetParameterByID(pair.Key);
                var sourceParam = this.DLGTree.GetParameterByID(pair.Value);
                if ( targetParam != null && sourceParam != null ) {
                    currentInstance.SetActorReference(targetParam.name, sourceParam.actor);
                }
            }
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        public override string GetConnectionInfo(int i) {
            return i == 0 ? "Success" : "Failure";
        }

        protected override void OnNodeInspectorGUI() {
            base.OnNodeInspectorGUI();
            if ( subGraph != null ) {
                ShowActorParametersMapping();
            }
        }

        //Shows actor parameters mapping
        void ShowActorParametersMapping() {
            EditorUtils.Separator();
            EditorUtils.CoolLabel("SubGraph Actor Parameters Mapping");
            UnityEditor.EditorGUILayout.HelpBox("Map SubGraph actor parameters from this graph actor parameters. Leaving [NONE] will not affect the parameter.", UnityEditor.MessageType.Info);

            if ( subGraph.actorParameters.Count == 0 ) {
                return;
            }

            if ( _actorParametersMap == null ) { _actorParametersMap = new Dictionary<string, string>(); }

            foreach ( var param in subGraph.actorParameters ) {
                if ( !_actorParametersMap.ContainsKey(param.ID) ) {
                    _actorParametersMap[param.ID] = string.Empty;
                }
                var currentParam = this.DLGTree.GetParameterByID(this._actorParametersMap[param.ID]);
                var newParam = EditorUtils.Popup<DialogueTree.ActorParameter>(param.name, currentParam, this.DLGTree.actorParameters);
                if ( newParam != currentParam ) {
                    this._actorParametersMap[param.ID] = newParam != null ? newParam.ID : string.Empty;
                }
            }

            foreach ( var key in _actorParametersMap.Keys.ToList() ) {
                if ( !subGraph.actorParameters.Select(p => p.ID).Contains(key) ) {
                    _actorParametersMap.Remove(key);
                }
            }
        }
#endif

    }
}