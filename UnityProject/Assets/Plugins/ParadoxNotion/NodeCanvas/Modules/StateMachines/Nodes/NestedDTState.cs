using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.StateMachines
{

    [Name("Sub Dialogue")]
    [Description("Execute the assigned Dialogue Tree OnEnter and stop it OnExit. Optionaly an event can be send for whether the dialogue ended in Success or Failure. This can be controled by using the 'Finish' Dialogue Node inside the Dialogue Tree. Use a 'CheckEvent' condition to make use of those events. The 'Instigator' Actor of the Dialogue Tree will be set to this graph's agent.")]
    [DropReferenceType(typeof(DialogueTree))]
    public class NestedDTState : FSMStateNested<DialogueTree>
    {

        [SerializeField, ExposeField, Name("Sub Tree")]
        private BBParameter<DialogueTree> _nestedDLG = null;

        [DimIfDefault] public string successEvent;
        [DimIfDefault] public string failureEvent;

        public override DialogueTree subGraph { get { return _nestedDLG.value; } set { _nestedDLG.value = value; } }
        public override BBParameter subGraphParameter => _nestedDLG;

        //		

        protected override void OnEnter() {
            if ( subGraph == null ) {
                Finish(false);
                return;
            }

            this.TryStartSubGraph(graphAgent, OnDialogueFinished);
        }

        protected override void OnUpdate() {
            currentInstance.UpdateGraph(this.graph.deltaTime);
        }

        protected override void OnExit() {
            if ( currentInstance != null ) {
                currentInstance.Stop();
            }
        }

        void OnDialogueFinished(bool success) {
            if ( this.status == Status.Running ) {
                if ( !string.IsNullOrEmpty(successEvent) && success ) {
                    SendEvent(successEvent);
                }

                if ( !string.IsNullOrEmpty(failureEvent) && !success ) {
                    SendEvent(failureEvent);
                }

                Finish(success);
            }
        }
    }
}