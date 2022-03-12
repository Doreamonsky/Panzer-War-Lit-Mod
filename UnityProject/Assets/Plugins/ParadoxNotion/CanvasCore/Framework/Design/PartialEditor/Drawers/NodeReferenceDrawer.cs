#if UNITY_EDITOR

using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using System.Linq;
using ParadoxNotion;

namespace NodeCanvas.Editor
{

    ///<summary>A drawer for INodeReference which is useful to weak reference nodes from within one another</summary>
    public class NodeReferenceDrawer : ObjectDrawer<INodeReference>
    {
        public override INodeReference OnGUI(GUIContent content, INodeReference instance) {
            //we presume that INodeRefence is serialized in a Node context
            if ( instance == null ) {
                UnityEditor.EditorGUILayout.LabelField(content.text, "Null NodeReference Instance");
                return instance;
            }
            var contextNode = context as Node;
            if ( contextNode == null || contextNode.graph == null ) { return instance; }
            var graph = contextNode.graph;

            var targets = graph.allNodes.Where(x => instance.type.IsAssignableFrom(x.GetType()));
            var current = instance.Get(graph);
            var newTarget = EditorUtils.Popup<Node>(content, current, targets);
            if ( newTarget != current ) {
                UndoUtility.RecordObject(contextUnityObject, "Set Node Reference");
                instance.Set(newTarget);
                foreach ( var callbackAtt in attributes.OfType<CallbackAttribute>() ) {
                    var m = contextNode.GetType().RTGetMethod(callbackAtt.methodName);
                    if ( m != null ) { m.Invoke(contextNode, null); }
                }
                UndoUtility.SetDirty(contextUnityObject);
            }

            return instance;
        }
    }
}

#endif