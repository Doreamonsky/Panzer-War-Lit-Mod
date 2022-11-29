using NodeCanvas.Framework;
using UnityEngine;


namespace NodeCanvas.DialogueTrees
{

    [System.Obsolete("Use Jumpers instead")]
    public class GoToNode : DTNode
    {

        [SerializeField]
        private DTNode _targetNode = null;

        public override int maxOutConnections { get { return 0; } }
        public override bool requireActorSelection { get { return false; } }

        protected override Status OnExecute(Component agent, IBlackboard bb) {
            if ( _targetNode == null ) {
                return Error("Target node of GOTO node is null");
            }

            DLGTree.EnterNode(_targetNode);
            return Status.Success;
        }
    }
}