#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEditor;
using UnityEngine;

namespace NodeCanvas.Editor
{

    //Events
    partial class GraphEditor
    {

        private static bool mouse2Down;

        ///<summary>Graph events BEFORE nodes</summary>
        static void HandlePreNodesGraphEvents(Graph graph, Vector2 canvasMousePos) {

            if ( e.button == 2 && e.type == EventType.MouseDown /*|| e.type == EventType.MouseUp*/ ) {
                UndoUtility.RecordObjectComplete(graph, "Graph Pan");
            }

            if ( e.type == EventType.MouseUp || e.type == EventType.KeyUp ) {
                SnapNodesToGrid(graph);
            }

            if ( e.type == EventType.KeyDown && e.keyCode == KeyCode.F && GUIUtility.keyboardControl == 0 ) {
                FocusSelection();
                e.Use();
            }

            if ( e.type == EventType.MouseDown && e.button == 2 && e.clickCount == 2 ) {
                FocusPosition(ViewToCanvas(e.mousePosition));
            }

            if ( e.type == EventType.ScrollWheel && GraphEditorUtility.allowClick ) {
                if ( canvasRect.Contains(e.mousePosition) ) {
                    var zoomDelta = e.shift ? 0.1f : 0.25f;
                    ZoomAt(e.mousePosition, -e.delta.y > 0 ? zoomDelta : -zoomDelta);
                }
            }

            if ( e.type == EventType.MouseDrag && e.alt && e.button == 1 ) {
                ZoomAt(new Vector2(screenWidth / 2, screenHeight / 2), e.delta.x / 100);
                e.Use();
            }

            if ( ( e.button == 2 && e.type == EventType.MouseDrag && canvasRect.Contains(e.mousePosition) ) ||
                ( ( e.type == EventType.MouseDown || e.type == EventType.MouseDrag ) && e.alt && e.isMouse ) ) {
                pan += e.delta;
                smoothPan = null;
                smoothZoomFactor = null;
                e.Use();
            }

            if ( e.type == EventType.MouseDown && e.button == 2 ) { mouse2Down = true; }
            if ( e.type == EventType.MouseUp && e.button == 2 ) { mouse2Down = false; }
            if ( e.alt || mouse2Down ) { EditorGUIUtility.AddCursorRect(new Rect(0, 0, screenWidth, screenHeight), MouseCursor.Pan); }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Graph events AFTER nodes</summary>
        static void HandlePostNodesGraphEvents(Graph graph, Vector2 canvasMousePos) {

            //Shortcuts
            if ( GUIUtility.keyboardControl == 0 ) {

                if ( e.type == EventType.ValidateCommand ) {
                    if ( e.commandName == "Copy" || e.commandName == "Cut" || e.commandName == "Paste" || e.commandName == "SoftDelete" || e.commandName == "Delete" || e.commandName == "Duplicate" ) {
                        e.Use();
                    }
                }

                if ( e.type == EventType.ExecuteCommand ) {

                    //COPY/CUT
                    if ( e.commandName == "Copy" || e.commandName == "Cut" ) {
                        List<Node> selection = null;
                        if ( GraphEditorUtility.activeNode != null ) {
                            selection = new List<Node> { GraphEditorUtility.activeNode };
                        }
                        if ( GraphEditorUtility.activeElements != null && GraphEditorUtility.activeElements.Count > 0 ) {
                            selection = GraphEditorUtility.activeElements.Cast<Node>().ToList();
                        }
                        if ( selection != null ) {
                            CopyBuffer.SetCache<Node[]>(Graph.CloneNodes(selection).ToArray());
                            if ( e.commandName == "Cut" ) {
                                foreach ( Node node in selection ) { graph.RemoveNode(node); }
                            }
                        }
                        e.Use();
                    }

                    //PASTE
                    if ( e.commandName == "Paste" ) {
                        if ( CopyBuffer.HasCache<Node[]>() ) {
                            TryPasteNodesInGraph(graph, CopyBuffer.GetCache<Node[]>(), canvasMousePos + new Vector2(500, 500) / graph.zoomFactor);
                        }
                        e.Use();
                    }

                    //DUPLICATE
                    if ( e.commandName == "Duplicate" ) {
                        if ( GraphEditorUtility.activeElements != null && GraphEditorUtility.activeElements.Count > 0 ) {
                            TryPasteNodesInGraph(graph, GraphEditorUtility.activeElements.OfType<Node>().ToArray(), default(Vector2));
                        }
                        if ( GraphEditorUtility.activeNode != null ) {
                            GraphEditorUtility.activeElement = GraphEditorUtility.activeNode.Duplicate(graph);
                        }
                        //Connections can't be duplicated by themselves. They do so as part of multiple node duplication (at least 2).
                        e.Use();
                    }

                    //DELETE
                    if ( e.commandName == "SoftDelete" || e.commandName == "Delete" ) {
                        if ( GraphEditorUtility.activeElements != null && GraphEditorUtility.activeElements.Count > 0 ) {
                            foreach ( var obj in GraphEditorUtility.activeElements.ToArray() ) {
                                if ( obj is Node ) {
                                    graph.RemoveNode(obj as Node);
                                }
                                if ( obj is Connection ) {
                                    graph.RemoveConnection(obj as Connection);
                                }
                            }
                            GraphEditorUtility.activeElements = null;
                        }

                        if ( GraphEditorUtility.activeNode != null ) {
                            graph.RemoveNode(GraphEditorUtility.activeNode);
                            GraphEditorUtility.activeElement = null;
                        }

                        if ( GraphEditorUtility.activeConnection != null ) {
                            graph.RemoveConnection(GraphEditorUtility.activeConnection);
                            GraphEditorUtility.activeElement = null;
                        }
                        e.Use();
                    }
                }
            }

            //No panel is obscuring
            if ( GraphEditorUtility.allowClick ) {

                if ( e.type == EventType.MouseDown && e.clickCount == 2 && e.button == 0 ) {
                    current.maximized = !current.maximized;
                    e.Use();
                }

                //Right click or shortcut canvas context menu. Opens browser for adding new nodes.
                var isContext = e.type == EventType.ContextClick && !e.alt;
                var isShortcut = e.type == EventType.KeyDown && e.keyCode == KeyCode.Space && GUIUtility.keyboardControl == 0 && !e.shift;
                if ( isContext || isShortcut ) {
                    GenericMenuBrowser.ShowAsync(e.mousePosition, "Add Node", graph.baseNodeType, () =>
                   {
                       var menu = GetAddNodeMenu(graph, canvasMousePos);
                       if ( CopyBuffer.TryGetCache<Node[]>(out Node[] copiedNodes) && copiedNodes.Length > 0 ) {
                           if ( copiedNodes[0].GetType().IsSubclassOf(graph.baseNodeType) ) {
                               menu.AddSeparator("/");
                               var suffix = copiedNodes.Length == 1 ? copiedNodes[0].GetType().FriendlyName() : copiedNodes.Length.ToString();
                               if ( copiedNodes.Length > 0 ) {
                                   menu.AddItem(new GUIContent(string.Format("Paste Node(s) ({0})", suffix)), false, () => { TryPasteNodesInGraph(graph, copiedNodes, canvasMousePos); });
                               }
                           }
                       }
                       return menu;
                   });
                    e.Use();
                }
            }
        }

        ///----------------------------------------------------------------------------------------------

        //Paste nodes in target graph
        static void TryPasteNodesInGraph(Graph graph, Node[] nodes, Vector2 originPosition) {
            var newNodes = Graph.CloneNodes(nodes.ToList(), graph, originPosition);
            GraphEditorUtility.activeElements = newNodes.Cast<IGraphElement>().ToList();
        }

        ///<summary>The final generic menu used for adding nodes in the canvas</summary>
        static GenericMenu GetAddNodeMenu(Graph graph, Vector2 canvasMousePos) {
            System.Action<System.Type> Selected = (type) => { GraphEditorUtility.activeElement = graph.AddNode(type, canvasMousePos); };
            var menu = EditorUtils.GetTypeSelectionMenu(graph.baseNodeType, Selected);
            menu = graph.CallbackOnCanvasContextMenu(menu, canvasMousePos);
            return menu;
        }
    }
}

#endif