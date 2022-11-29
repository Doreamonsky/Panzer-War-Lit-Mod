using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees
{

    [Name("Sequencer", 10)]
    [Category("Composites")]
    [Description("Execute the child nodes in order and returns Success if all children return Success. As soon as a child node returns Failure, the Sequencer will stop and return Failure as well.")]
    [ParadoxNotion.Design.Icon("Sequencer")]
    [Color("bf7fff")]
    public class Sequencer : BTComposite
    {

        [Tooltip("If true, then higher priority child nodes are re-evaluated per frame and if either returns Failure, then the Sequencer will immediately stop and return Failure as well.")]
        public bool dynamic;
        [Tooltip("If true, the children order of execution is shuffled each time the Sequencer resets.")]
        public bool random;

        private int lastRunningNodeIndex = 0;

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            for ( var i = dynamic ? 0 : lastRunningNodeIndex; i < outConnections.Count; i++ ) {

                status = outConnections[i].Execute(agent, blackboard);

                switch ( status ) {
                    case Status.Running:

                        if ( dynamic && i < lastRunningNodeIndex ) {
                            for ( var j = i + 1; j <= lastRunningNodeIndex; j++ ) {
                                outConnections[j].Reset();
                            }
                        }

                        lastRunningNodeIndex = i;
                        return Status.Running;

                    case Status.Failure:

                        if ( dynamic && i < lastRunningNodeIndex ) {
                            for ( var j = i + 1; j <= lastRunningNodeIndex; j++ ) {
                                outConnections[j].Reset();
                            }
                        }

                        return Status.Failure;
                }
            }

            return Status.Success;
        }

        protected override void OnReset() {
            lastRunningNodeIndex = 0;
            if ( random ) { outConnections = outConnections.Shuffle(); }
        }

        public override void OnChildDisconnected(int index) {
            if ( index != 0 && index == lastRunningNodeIndex ) {
                lastRunningNodeIndex--;
            }
        }

        public override void OnGraphStarted() { OnReset(); }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR
        protected override void OnNodeGUI() {
            if ( dynamic ) { GUILayout.Label("<b>DYNAMIC</b>"); }
            if ( random ) { GUILayout.Label("<b>RANDOM</b>"); }
        }
#endif

    }
}