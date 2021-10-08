#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;


namespace NodeCanvas.Framework
{

    partial class Graph
    {

        private int _childAssignableIndex = -1;

        ///EDITOR. Responsible for breacrumb navigation only
        public Graph GetCurrentChildGraph() {
            if ( _childAssignableIndex == -1 || _childAssignableIndex > allNodes.Count - 1 ) {
                return null;
            }
            var assignable = allNodes[_childAssignableIndex] as IGraphAssignable;
            if ( assignable != null ) {
                return assignable.subGraph;
            }
            return null;
        }

        ///EDITOR. Responsible for breacrumb navigation only
        public void SetCurrentChildGraphAssignable(IGraphAssignable assignable) {
            if ( assignable == null || assignable.subGraph == null ) {
                _childAssignableIndex = -1;
                return;
            }
            if ( Application.isPlaying && EditorUtility.IsPersistent(assignable.subGraph) ) {
                ParadoxNotion.Services.Logger.LogWarning("You can't view sub-graphs in play mode until they are initialized to avoid editing asset references accidentally", LogTag.EDITOR, this);
                _childAssignableIndex = -1;
                return;
            }
            assignable.subGraph.SetCurrentChildGraphAssignable(null);
            _childAssignableIndex = allNodes.IndexOf(assignable as Node);
        }

        ///----------------------------------------------------------------------------------------------

        internal GenericMenu CallbackOnCanvasContextMenu(GenericMenu menu, Vector2 canvasMousePos) { return OnCanvasContextMenu(menu, canvasMousePos); }
        internal GenericMenu CallbackOnNodesContextMenu(GenericMenu menu, Node[] nodes) { return OnNodesContextMenu(menu, nodes); }
        internal void CallbackOnDropAccepted(Object o, Vector2 canvasMousePos) {
            ///for all graphs, make possible to drag and drop IGraphAssignables
            foreach ( var type in Editor.GraphEditorUtility.GetDropedReferenceNodeTypes<IGraphAssignable>(o) ) {
                if ( baseNodeType.IsAssignableFrom(type) ) {
                    var node = (IGraphAssignable)AddNode(type, canvasMousePos);
                    node.subGraph = (Graph)o;
                    return;
                }
            }
            OnDropAccepted(o, canvasMousePos);
        }
        internal void CallbackOnVariableDropInGraph(IBlackboard bb, Variable variable, Vector2 canvasMousePos) { OnVariableDropInGraph(bb, variable, canvasMousePos); }
        internal void CallbackOnGraphEditorToolbar() { OnGraphEditorToolbar(); }

        ///----------------------------------------------------------------------------------------------

        ///Editor. Override to add extra context sensitive options in the right click canvas context menu
        virtual protected GenericMenu OnCanvasContextMenu(GenericMenu menu, Vector2 canvasMousePos) { return menu; }
        ///Editor. Override to add more entries to the right click context menu when multiple nodes are selected
        virtual protected GenericMenu OnNodesContextMenu(GenericMenu menu, Node[] nodes) { return menu; }
        ///Editor. Handle drag and drop objects in the graph
        virtual protected void OnDropAccepted(Object o, Vector2 canvasMousePos) { }
        ///Editor. Handle what happens when blackboard variable is drag and droped in graph
        virtual protected void OnVariableDropInGraph(IBlackboard bb, Variable variable, Vector2 canvasMousePos) { }
        ///Editor. Append stuff in graph editor toolbar
        virtual protected void OnGraphEditorToolbar() { }

        ///----------------------------------------------------------------------------------------------

    }
}

#endif
