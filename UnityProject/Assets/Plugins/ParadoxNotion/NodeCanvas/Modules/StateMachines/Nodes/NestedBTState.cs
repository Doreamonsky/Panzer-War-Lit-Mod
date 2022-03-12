using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.StateMachines
{

    [Name("Sub BehaviourTree")]
    [Description("Execute a Behaviour Tree OnEnter. OnExit that Behavior Tree will be stoped or paused based on the relevant specified setting. You can optionaly specify a Success Event and a Failure Event which will be sent when the BT's root node status returns either of the two. If so, use alongside with a CheckEvent on a transition.")]
    [DropReferenceType(typeof(BehaviourTree))]
    public class NestedBTState : FSMStateNested<BehaviourTree>
    {

        public enum BTExecutionMode
        {
            Once,
            Repeat
        }

        public enum BTExitMode
        {
            StopAndRestart,
            PauseAndResume
        }

        [SerializeField, ExposeField, Name("Sub Tree")]
        private BBParameter<BehaviourTree> _nestedBT = null;
        public BTExitMode exitMode = BTExitMode.StopAndRestart;
        public BTExecutionMode executionMode = BTExecutionMode.Repeat;

        [DimIfDefault] public string successEvent;
        [DimIfDefault] public string failureEvent;

        public override BehaviourTree subGraph { get { return _nestedBT.value; } set { _nestedBT.value = value; } }
        public override BBParameter subGraphParameter => _nestedBT;

        //

        protected override void OnEnter() {

            if ( subGraph == null ) {
                Finish(false);
                return;
            }

            currentInstance = (BehaviourTree)this.CheckInstance();
            currentInstance.repeat = ( executionMode == BTExecutionMode.Repeat );
            currentInstance.updateInterval = 0;
            this.TryWriteAndBindMappedVariables();
            currentInstance.StartGraph(graph.agent, graph.blackboard.parent, Graph.UpdateMode.Manual, OnFinish);
            OnUpdate();
        }

        protected override void OnUpdate() {

            currentInstance.UpdateGraph(this.graph.deltaTime);

            if ( !string.IsNullOrEmpty(successEvent) && currentInstance.rootStatus == Status.Success ) {
                currentInstance.Stop(true);
            }

            if ( !string.IsNullOrEmpty(failureEvent) && currentInstance.rootStatus == Status.Failure ) {
                currentInstance.Stop(false);
            }
        }

        void OnFinish(bool success) {
            if ( this.status == Status.Running ) {

                this.TryReadAndUnbindMappedVariables();

                if ( !string.IsNullOrEmpty(successEvent) && success ) {
                    SendEvent(successEvent);
                }

                if ( !string.IsNullOrEmpty(failureEvent) && !success ) {
                    SendEvent(failureEvent);
                }

                Finish(success);
            }
        }

        protected override void OnExit() {
            if ( currentInstance != null ) {
                if ( this.status == Status.Running ) {
                    this.TryReadAndUnbindMappedVariables();
                }
                if ( exitMode == BTExitMode.StopAndRestart ) {
                    currentInstance.Stop();
                } else {
                    currentInstance.Pause();
                }
            }
        }
    }
}