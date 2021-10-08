using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using NodeCanvas.BehaviourTrees;

namespace NodeCanvas.StateMachines
{

    [Name("Parallel Sub BehaviourTree", -1)]
    [Description("Execute a Sub Behaviour Tree in parallel and for as long as this FSM is running.")]
    [Category("SubGraphs")]
    [Color("ff64cb")]
    public class ConcurrentSubTree : FSMNodeNested<BehaviourTree>, IUpdatable
    {

        [SerializeField, ExposeField, Name("Parallel Tree")]
        protected BBParameter<BehaviourTree> _subTree = null;

        public override string name => base.name.ToUpper();
        public override int maxInConnections => 0;
        public override int maxOutConnections => 0;
        public override bool allowAsPrime => false;

        public override BehaviourTree subGraph { get { return _subTree.value; } set { _subTree.value = value; } }
        public override BBParameter subGraphParameter => _subTree;

        ///----------------------------------------------------------------------------------------------

        public override void OnGraphStarted() {
            if ( subGraph == null ) { return; }
            status = Status.Running;
            this.TryStartSubGraph(graphAgent, (result) => { status = result ? Status.Success : Status.Failure; });
        }

        void IUpdatable.Update() {
            this.TryUpdateSubGraph();
        }
    }
}