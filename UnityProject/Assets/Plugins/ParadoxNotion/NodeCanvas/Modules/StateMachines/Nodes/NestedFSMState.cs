using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.StateMachines
{


    [Name("Sub FSM")]
    [Description("Execute a nested FSM OnEnter and Stop that FSM OnExit. This state is Finished when the nested FSM is finished as well")]
    [DropReferenceType(typeof(FSM))]
    public class NestedFSMState : FSMStateNested<FSM>
    {

        public enum FSMExitMode
        {
            StopAndRestart,
            PauseAndResume
        }

        [SerializeField, ExposeField, Name("Sub FSM")]
        protected BBParameter<FSM> _nestedFSM = null; //protected so that derived user types can be reflected correctly
        public FSMExitMode exitMode;

        public override FSM subGraph { get { return _nestedFSM.value; } set { _nestedFSM.value = value; } }
        public override BBParameter subGraphParameter => _nestedFSM;

        //

        protected override void OnEnter() {
            if ( subGraph == null ) {
                Finish(false);
                return;
            }

            this.TryStartSubGraph(graphAgent, Finish);
            OnUpdate();
        }

        protected override void OnUpdate() {
            currentInstance.UpdateGraph(this.graph.deltaTime);
        }

        protected override void OnExit() {
            if ( currentInstance != null ) {
                if ( this.status == Status.Running ) {
                    this.TryReadAndUnbindMappedVariables();
                }
                if ( exitMode == FSMExitMode.StopAndRestart ) {
                    currentInstance.Stop();
                } else {
                    currentInstance.Pause();
                }
            }
        }
    }
}