using NodeCanvas.Framework;
using ParadoxNotion;
using UnityEngine;

namespace NodeCanvas.StateMachines
{


    ///<summary> Super base class for FSM nodes that live within an FSM Graph.</summary>
    public abstract class FSMNode : Node
    {

        public override bool allowAsPrime { get { return false; } }
        public override bool canSelfConnect { get { return false; } }
        public override int maxInConnections { get { return -1; } }
        public override int maxOutConnections { get { return -1; } }
        sealed public override System.Type outConnectionType { get { return typeof(FSMConnection); } }
        sealed public override Alignment2x2 commentsAlignment { get { return Alignment2x2.Bottom; } }
        sealed public override Alignment2x2 iconAlignment { get { return Alignment2x2.Default; } }

        ///<summary>The FSM this state belongs to</summary>
        public FSM FSM { get { return (FSM)graph; } }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        private static GUIPort clickedPort { get; set; }
        private static int dragDropMisses { get; set; }

        class GUIPort
        {
            public FSMNode parent { get; private set; }
            public Vector2 pos { get; private set; }
            public GUIPort(FSMNode parent, Vector2 pos) {
                this.parent = parent;
                this.pos = pos;
            }
        }

        //Draw the ports and connections
        sealed protected override void DrawNodeConnections(Rect drawCanvas, bool fullDrawPass, Vector2 canvasMousePos, float zoomFactor) {

            var e = Event.current;

            //Receive connections first
            if ( clickedPort != null && e.type == EventType.MouseUp && e.button == 0 ) {

                if ( rect.Contains(e.mousePosition) ) {
                    graph.ConnectNodes(clickedPort.parent, this);
                    clickedPort = null;
                    e.Use();

                } else {

                    dragDropMisses++;

                    if ( dragDropMisses == graph.allNodes.Count && clickedPort != null ) {
                        var source = clickedPort.parent;
                        var pos = Event.current.mousePosition;
                        var menu = new UnityEditor.GenericMenu();
                        clickedPort = null;

                        menu.AddItem(new GUIContent("Add Action State"), false, () =>
                        {
                            var newState = graph.AddNode<ActionState>(pos);
                            graph.ConnectNodes(source, newState);
                        });

                        //PostGUI cause of zoom factors
                        Editor.GraphEditorUtility.PostGUI += () => { menu.ShowAsContext(); };
                        Event.current.Use();
                        e.Use();
                    }
                }
            }

            var portRectLeft = new Rect(0, 0, 20, 20);
            var portRectRight = new Rect(0, 0, 20, 20);
            var portRectBottom = new Rect(0, 0, 20, 20);

            portRectLeft.center = new Vector2(rect.x - 11, rect.center.y);
            portRectRight.center = new Vector2(rect.xMax + 11, rect.center.y);
            portRectBottom.center = new Vector2(rect.center.x, rect.yMax + 11);

            if ( maxOutConnections != 0 ) {
                if ( fullDrawPass || drawCanvas.Overlaps(rect) ) {
                    UnityEditor.EditorGUIUtility.AddCursorRect(portRectLeft, UnityEditor.MouseCursor.ArrowPlus);
                    UnityEditor.EditorGUIUtility.AddCursorRect(portRectRight, UnityEditor.MouseCursor.ArrowPlus);
                    UnityEditor.EditorGUIUtility.AddCursorRect(portRectBottom, UnityEditor.MouseCursor.ArrowPlus);

                    GUI.color = new Color(1, 1, 1, 0.3f);
                    GUI.DrawTexture(portRectLeft, Editor.StyleSheet.arrowLeft);
                    GUI.DrawTexture(portRectRight, Editor.StyleSheet.arrowRight);
                    if ( maxInConnections == 0 ) {
                        GUI.DrawTexture(portRectBottom, Editor.StyleSheet.arrowBottom);
                    }
                    GUI.color = Color.white;

                    if ( Editor.GraphEditorUtility.allowClick && e.type == EventType.MouseDown && e.button == 0 ) {

                        if ( portRectLeft.Contains(e.mousePosition) ) {
                            clickedPort = new GUIPort(this, portRectLeft.center);
                            dragDropMisses = 0;
                            e.Use();
                        }

                        if ( portRectRight.Contains(e.mousePosition) ) {
                            clickedPort = new GUIPort(this, portRectRight.center);
                            dragDropMisses = 0;
                            e.Use();
                        }

                        if ( maxInConnections == 0 && portRectBottom.Contains(e.mousePosition) ) {
                            clickedPort = new GUIPort(this, portRectBottom.center);
                            dragDropMisses = 0;
                            e.Use();
                        }
                    }
                }
            }

            //draw new linking
            if ( clickedPort != null && clickedPort.parent == this ) {
                UnityEditor.Handles.DrawBezier(clickedPort.pos, e.mousePosition, clickedPort.pos, e.mousePosition, new Color(0.5f, 0.5f, 0.8f, 0.8f), Editor.StyleSheet.bezierTexture, 2);
            }

            //draw out connections
            for ( var i = 0; i < outConnections.Count; i++ ) {

                var connection = outConnections[i] as FSMConnection;
                var targetState = connection.targetNode as FSMNode;
                if ( targetState == null ) { //In case of MissingNode type
                    continue;
                }

                var targetPos = targetState.GetConnectedInPortPosition(connection);
                var sourcePos = Vector2.zero;

                if ( rect.center.x <= targetPos.x ) {
                    sourcePos = portRectRight.center;
                }

                if ( rect.center.x > targetPos.x ) {
                    sourcePos = portRectLeft.center;
                }

                if ( maxInConnections == 0 && rect.center.y < targetPos.y - 50 && Mathf.Abs(rect.center.x - targetPos.x) < 200 ) {
                    sourcePos = portRectBottom.center;
                }

                var boundRect = RectUtils.GetBoundRect(sourcePos, targetPos);
                if ( fullDrawPass || drawCanvas.Overlaps(boundRect) ) {
                    connection.DrawConnectionGUI(sourcePos, targetPos);
                }
            }
        }


        //...
        Vector2 GetConnectedInPortPosition(Connection connection) {

            var sourcePos = connection.sourceNode.rect.center;
            var thisPos = rect.center;

            var style = 0;

            if ( style == 0 ) {
                if ( sourcePos.x <= thisPos.x ) {
                    if ( sourcePos.y <= thisPos.y ) {
                        return new Vector2(rect.center.x - 15, rect.yMin - ( this == graph.primeNode ? 20 : 0 ));
                    } else {
                        return new Vector2(rect.center.x - 15, rect.yMax + 2);
                    }
                }

                if ( sourcePos.x > thisPos.x ) {
                    if ( sourcePos.y <= thisPos.y ) {
                        return new Vector2(rect.center.x + 15, rect.yMin - ( this == graph.primeNode ? 20 : 0 ));
                    } else {
                        return new Vector2(rect.center.x + 15, rect.yMax + 2);
                    }
                }
            }

            // //Another idea
            // if (style == 1){
            // 	if (sourcePos.x <= thisPos.x){
            // 		if (sourcePos.y >= thisPos.y){
            // 			return new Vector2(rect.xMin - 3, rect.yMax - 10);
            // 		} else {
            // 			return new Vector2(rect.xMin - 3, rect.yMin + 10);
            // 		}
            // 	}
            // 	if (sourcePos.x > thisPos.x){
            // 		if (sourcePos.y >= thisPos.y){
            // 			return new Vector2(rect.center.x, rect.yMax + 2);
            // 		} else {
            // 			return new Vector2(rect.center.x, rect.yMin - (this == graph.primeNode? 20 : 0 ));
            // 		}
            // 	}
            // }

            // //YET Another idea
            // if (style >= 2){
            // 	if (sourcePos.x <= thisPos.x){
            // 		if (sourcePos.y >= thisPos.y){
            // 			return new Vector2(rect.xMin - 3, rect.yMax - 10);
            // 		} else {
            // 			return new Vector2(rect.xMin - 3, rect.yMin + 10);
            // 		}
            // 	}
            // 	if (sourcePos.x > thisPos.x){
            // 		if (sourcePos.y >= thisPos.y){
            // 			return new Vector2(rect.xMax + 3, rect.yMax - 10);
            // 		} else {
            // 			return new Vector2(rect.xMax + 3, rect.yMin + 10);
            // 		}
            // 	}
            // }

            return thisPos;
        }


#endif

    }
}