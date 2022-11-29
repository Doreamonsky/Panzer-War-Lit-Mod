using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Sub Tree")]
    [Description("SubTree Node can be assigned an entire Sub BehaviorTree. The status of the root node in the SubTree will be returned.")]
    [ParadoxNotion.Design.Icon("BT")]
    [DropReferenceType(typeof(BehaviourTree))]
    public class SubTree : BTNodeNested<BehaviourTree>
    {

        [SerializeField, ExposeField]
        private BBParameter<BehaviourTree> _subTree = null;

        public override BehaviourTree subGraph { get { return _subTree.value; } set { _subTree.value = value; } }
        public override BBParameter subGraphParameter => _subTree;

        ///----------------------------------------------------------------------------------------------

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( subGraph == null || subGraph.primeNode == null ) {
                return Status.Optional;
            }

            if ( status == Status.Resting ) {
                this.TryStartSubGraph(agent);
            }

            currentInstance.UpdateGraph(this.graph.deltaTime);

            if ( currentInstance.repeat && currentInstance.rootStatus != Status.Running ) {
                this.TryReadAndUnbindMappedVariables();
            }

            return currentInstance.rootStatus;
        }

        protected override void OnReset() {
            if ( currentInstance != null ) {
                currentInstance.Stop();
            }
        }
    }
}