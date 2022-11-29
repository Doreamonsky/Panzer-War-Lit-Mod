using System.Linq;
using ParadoxNotion;
using UnityEngine;


namespace NodeCanvas.Framework
{

    ///<summary> A rect group within a Graph</summary>
	[System.Serializable]
    public class CanvasGroup
    {

        public string name;
        public Rect rect;
        public Color color;
        public bool autoGroup;

        //required
        public CanvasGroup() { }
        public CanvasGroup(Rect rect, string name) {
            this.rect = rect;
            this.name = name;
        }

#if UNITY_EDITOR
        [System.NonSerialized] public EditState editState;
        [System.NonSerialized] private Node[] containedNodes;

        public enum EditState
        {
            None, Dragging, Renaming, Scaling
        }

        public Node[] GatherContainedNodes(Graph graph) {
            containedNodes = graph.allNodes.Where(n => rect.Encapsulates(n.rect)).ToArray();
            return containedNodes;
        }

        public Rect AdjustToContainedNodes() {
            if ( autoGroup && containedNodes != null && containedNodes.Length > 0 ) {
                rect = RectUtils.GetBoundRect(containedNodes.Select(n => n.rect).ToArray()).ExpandBy(30, 65, 30, 30);
            }
            return rect;
        }

        public void FlushContainedNodes() {
            containedNodes = null;
        }

        public void GatherAdjustAndFlushContainedNodes(Graph graph) {
            GatherContainedNodes(graph);
            AdjustToContainedNodes();
            FlushContainedNodes();
        }
#endif

    }
}