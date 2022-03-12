using ParadoxNotion;
using NodeCanvas.Framework;
using System.Linq;

namespace NodeCanvas.BehaviourTrees
{

    ///<summary> Base class for BehaviourTree Decorator nodes.</summary>
    abstract public class BTDecorator : BTNode
    {

        sealed public override int maxOutConnections { get { return 1; } }
        sealed public override Alignment2x2 commentsAlignment { get { return Alignment2x2.Right; } }

        ///<summary>The decorated connection element</summary>
        protected Connection decoratedConnection {
            get { return outConnections.Count > 0 ? outConnections[0] : null; }
        }

        ///<summary>The decorated node element</summary>
        protected Node decoratedNode {
            get
            {
                var c = decoratedConnection;
                return c != null ? c.targetNode : null;
            }
        }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        protected override UnityEditor.GenericMenu OnContextMenu(UnityEditor.GenericMenu menu) {
            menu = base.OnContextMenu(menu);
            menu = ParadoxNotion.Design.EditorUtils.GetTypeSelectionMenu(typeof(BTDecorator), (t) => { this.ReplaceWith(t); }, menu, "Replace");
            return menu;
        }

#endif

    }
}