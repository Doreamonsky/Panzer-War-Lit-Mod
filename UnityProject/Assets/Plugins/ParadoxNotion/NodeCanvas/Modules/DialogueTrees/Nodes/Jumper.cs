using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.DialogueTrees
{

    [Name("JUMP")]
    [Description("Select a target node to jump to.\nFor your convenience in identifying nodes in the dropdown, please give a Tag name to the nodes you want to use in this way.")]
    [Category("Control")]
    [ParadoxNotion.Design.Icon("Set")]
    [Color("00b9e8")]
    public class Jumper : DTNode, IHaveNodeReference
    {
        [ParadoxNotion.Serialization.FullSerializer.fsSerializeAs("_sourceNodeUID")]
        public NodeReference<DTNode> _targetNode;

        INodeReference IHaveNodeReference.targetReference => _targetNode;
        private DTNode target => _targetNode?.Get(graph);

        public override int maxOutConnections { get { return 0; } }
        public override bool requireActorSelection { get { return false; } }

        protected override Status OnExecute(Component agent, IBlackboard bb) {
            if ( target == null ) { return Error("Target Node of Jumper node is null"); }
            DLGTree.EnterNode(target);
            return Status.Success;
        }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR
        protected override void OnNodeGUI() {
            GUILayout.Label(string.Format("<b>{0}</b>", target != null ? target.ToString() : "NONE"));
        }
#endif

    }
}