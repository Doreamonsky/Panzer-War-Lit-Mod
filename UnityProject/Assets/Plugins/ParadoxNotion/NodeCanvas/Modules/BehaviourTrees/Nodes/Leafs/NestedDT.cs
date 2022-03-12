using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Sub Dialogue")]
    [Description("Will Execute the nested dialogue assigned and return Success or Failure depending on the Dialogue Tree end state. This can be controled by using the 'Finish' Dialogue Node inside the Dialogue Tree. By default a Dialogue Tree ends in Success. The 'Instigator' Actor of the Dialogue Tree will be set to this graph's agent.")]
    [ParadoxNotion.Design.Icon("Dialogue")]
    [DropReferenceType(typeof(DialogueTree))]
    public class NestedDT : BTNodeNested<DialogueTree>
    {

        [SerializeField, ExposeField, Name("Sub Tree")]
        private BBParameter<DialogueTree> _nestedDialogueTree = null;

        public override DialogueTree subGraph { get { return _nestedDialogueTree.value; } set { _nestedDialogueTree.value = value; } }
        public override BBParameter subGraphParameter => _nestedDialogueTree;

        //

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( subGraph == null || subGraph.primeNode == null ) {
                return Status.Optional;
            }

            if ( status == Status.Resting ) {
                status = Status.Running;
                this.TryStartSubGraph(agent, OnDLGFinished);
            }

            if ( status == Status.Running ) {
                currentInstance.UpdateGraph(this.graph.deltaTime);
            }

            return status;
        }

        void OnDLGFinished(bool success) {
            if ( status == Status.Running ) {
                status = success ? Status.Success : Status.Failure;
            }
        }

        protected override void OnReset() {
            if ( currentInstance != null ) {
                currentInstance.Stop();
            }
        }
    }
}