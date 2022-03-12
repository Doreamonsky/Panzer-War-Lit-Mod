using System.Linq;
using NodeCanvas.Framework;
using NodeCanvas.StateMachines;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Sub FSM")]
    [Description("Executes a Sub FSM. This node will return Running for as long as the FSM is Running. If a Success or Failure State is selected, then it will return Success or Failure as soon as the Nested FSM enters that state at which point the FSM will also be stoped. If the Nested FSM ends otherwise, this node will return Success.")]
    [ParadoxNotion.Design.Icon("FSM")]
    [DropReferenceType(typeof(FSM))]
    public class NestedFSM : BTNodeNested<FSM>
    {

        [SerializeField, ExposeField, Name("Sub FSM")]
        private BBParameter<FSM> _nestedFSM = null;

        [HideInInspector] public string successState;
        [HideInInspector] public string failureState;

        public override FSM subGraph { get { return _nestedFSM.value; } set { _nestedFSM.value = value; } }
        public override BBParameter subGraphParameter => _nestedFSM;

        ///----------------------------------------------------------------------------------------------

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( subGraph == null || subGraph.primeNode == null ) {
                return Status.Optional;
            }

            if ( status == Status.Resting ) {
                status = Status.Running;
                this.TryStartSubGraph(agent, OnFSMFinish);
            }

            if ( status == Status.Running ) {
                currentInstance.UpdateGraph(this.graph.deltaTime);
            }

            if ( !string.IsNullOrEmpty(successState) && currentInstance.currentStateName == successState ) {
                currentInstance.Stop(true);
                return Status.Success;
            }

            if ( !string.IsNullOrEmpty(failureState) && currentInstance.currentStateName == failureState ) {
                currentInstance.Stop(false);
                return Status.Failure;
            }

            return status;
        }

        void OnFSMFinish(bool success) {
            if ( status == Status.Running ) {
                status = success ? Status.Success : Status.Failure;
            }
        }

        protected override void OnReset() {
            if ( currentInstance != null ) {
                currentInstance.Stop();
            }
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR
        protected override void OnNodeInspectorGUI() {
            base.OnNodeInspectorGUI();
            if ( subGraph != null ) {
                successState = EditorUtils.Popup<string>("Success State", successState, subGraph.GetStateNames());
                failureState = EditorUtils.Popup<string>("Failure State", failureState, subGraph.GetStateNames());
            }
        }
#endif
        ///----------------------------------------------------------------------------------------------

    }
}