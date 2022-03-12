#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using ParadoxNotion;
using ParadoxNotion.Design;
using NodeCanvas.Framework;

namespace NodeCanvas.Editor
{

    public class GraphExplorer : EditorWindow
    {

        const int INDENT_WIDTH = 25;
        const int INDENT_START = 1;

        private HierarchyTree.Element lastHoverElement;
        private string search;
        private Vector2 scrollPos;
        private bool willRepaint;
        private int indent;

        ///----------------------------------------------------------------------------------------------

        ///<summary>Show the finder window</summary>
        public static void ShowWindow() {
            GetWindow<GraphExplorer>().Show();
        }

        //...
        void OnEnable() {
            titleContent = new GUIContent("Explorer", StyleSheet.canvasIcon);

            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
            Graph.onGraphSerialized -= OnGraphSerialized;
            Graph.onGraphSerialized += OnGraphSerialized;
            GraphEditor.onCurrentGraphChanged -= GraphChanged;
            GraphEditor.onCurrentGraphChanged += GraphChanged;
            GraphEditorUtility.onActiveElementChanged -= OnActiveElementChanged;
            GraphEditorUtility.onActiveElementChanged += OnActiveElementChanged;

            willRepaint = true;
        }

        //...
        void OnDisable() {
            Graph.onGraphSerialized -= OnGraphSerialized;
            GraphEditor.onCurrentGraphChanged -= GraphChanged;
            GraphEditorUtility.onActiveElementChanged -= OnActiveElementChanged;
        }

        //...
        void GraphChanged(Graph graph) {
            search = null;
            GUIUtility.hotControl = 0;
            GUIUtility.keyboardControl = 0;
            willRepaint = true;
        }

        //...
        void OnActiveElementChanged(IGraphElement element) {
            willRepaint = true;
        }

        //...
        void OnGraphSerialized(Graph graph) {
            willRepaint = true;
        }

        ///----------------------------------------------------------------------------------------------

        //...
        void Update() {
            if ( willRepaint ) {
                willRepaint = false;
                Repaint();
            }
        }

        //...
        void OnGUI() {

            if ( GraphEditor.current == null || GraphEditor.currentGraph == null ) {
                ShowNotification(new GUIContent("No Graph is currently open in the Graph Editor"));
                return;
            } else {
                RemoveNotification();
            }

            EditorGUILayout.HelpBox("A flat meta graph structure including nodes, connections, tasks and parameters. This is not a hierarchical tree representation of the graph. Use this utility window to quickly search, find and jump focus to the related element. Please note that keeping this utility window open, will slow down the graph editor.", MessageType.Info);

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUI.SetNextControlName("SearchToolbar");
            search = EditorUtils.SearchField(search);
            Prefs.explorerShowTypeNames = EditorGUILayout.ToggleLeft("Show Type Names", Prefs.explorerShowTypeNames, GUILayout.Width(130));
            GUILayout.EndHorizontal();

            var graphElement = GraphEditor.currentGraph.GetFlatMetaGraph();
            if ( graphElement == null ) {
                return;
            }

            EditorUtils.BoldSeparator();

            GUILayout.Label(string.Format("<size=12><b> ROOT</b></size>", GraphEditor.currentGraph.name));
            EditorUtils.Separator();

            ///----------------------------------------------------------------------------------------------
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
            indent = INDENT_START;
            DoElement(graphElement);
            indent = INDENT_START;
            EditorGUILayout.EndScrollView();
            ///----------------------------------------------------------------------------------------------

            if ( Event.current.type == EventType.KeyDown ) {
                EditorGUI.FocusTextInControl("SearchToolbar");
                Event.current.Use();
            }

            if ( Event.current.type == EventType.MouseLeaveWindow ) {
                willRepaint = true;
            }
        }

        //...
        void DoElement(HierarchyTree.Element element, Rect parentElementRect = default(Rect)) {

            if ( element.children == null ) { return; }

            foreach ( var child in element.children ) {

                var elementRect = default(Rect);

                if ( child.reference == null ) { continue; }

                //Dont show undefined parameters.
                //TODO: I dont like this "special case" here
                if ( child.reference is BBParameter ) {
                    var bbPram = (BBParameter)child.reference;
                    if ( !bbPram.isDefined ) { continue; }
                }

                var toString = child.reference.ToString();
                var typeName = child.reference.GetType().FriendlyName();
                var searchText = toString + " " + typeName;

                if ( string.IsNullOrEmpty(search) || StringUtils.SearchMatch(search, searchText) ) {

                    if ( EditorGUIUtility.isProSkin ) { GUI.color = Color.black.WithAlpha(indent == 1 ? 0.6f : 0.3f); }
                    if ( !EditorGUIUtility.isProSkin ) { GUI.color = Color.white.WithAlpha(indent == 1 ? 0.6f : 0.3f); }
                    GUILayout.BeginHorizontal("box");
                    GUI.color = Color.white;
                    GUILayout.Space(indent * INDENT_WIDTH);
                    var displayText = string.Format("<b>{0}</b>{1}", toString, Prefs.explorerShowTypeNames ? " (" + typeName + ")" : string.Empty);
                    GUILayout.Label(string.Format("<size=9>{0}</size>", displayText));
                    GUILayout.EndHorizontal();

                    elementRect = GUILayoutUtility.GetLastRect();

                    EditorGUIUtility.AddCursorRect(elementRect, MouseCursor.Link);
                    if ( elementRect.Contains(Event.current.mousePosition) ) {
                        if ( child != lastHoverElement ) {
                            lastHoverElement = child;
                            willRepaint = true;
                            PingElement(child);
                        }
                        GUI.color = new Color(0.5f, 0.5f, 1, 0.3f);
                        GUI.DrawTexture(elementRect, EditorGUIUtility.whiteTexture);
                        GUI.color = Color.white;
                        if ( Event.current.type == EventType.MouseDown ) {
                            FocusElement(child);
                            Event.current.Use();
                        }
                    }

                    if ( GraphEditorUtility.activeElement == child.reference ) {
                        GUI.color = new Color(0.5f, 0.5f, 1, 0.1f);
                        GUI.DrawTexture(elementRect, EditorGUIUtility.whiteTexture);
                        GUI.color = Color.white;
                    }
                }

                indent++;
                DoElement(child, elementRect);
                indent--;

                if ( elementRect != default(Rect) ) {
                    var rootOrNotParentHidden = indent == INDENT_START || parentElementRect != default(Rect);

                    var lineVer = new Rect();
                    lineVer.xMin = elementRect.xMin + ( indent * INDENT_WIDTH ) - ( INDENT_WIDTH / 2 );
                    lineVer.width = 2;
                    lineVer.yMin = parentElementRect.yMax + 6;
                    lineVer.yMax = elementRect.yMax - ( elementRect.height / 2 );

                    var lineHor = new Rect();
                    lineHor.xMin = rootOrNotParentHidden ? lineVer.xMin : ( INDENT_WIDTH / 2 );
                    lineHor.xMax = lineVer.xMin + ( INDENT_WIDTH / 2 );
                    lineHor.yMin = lineVer.yMax - 2;
                    lineHor.height = 2;

                    GUI.color = Colors.Grey(EditorGUIUtility.isProSkin ? 0.6f : 0.3f);
                    if ( rootOrNotParentHidden ) {
                        GUI.DrawTexture(lineVer, Texture2D.whiteTexture);
                    }
                    GUI.DrawTexture(lineHor, Texture2D.whiteTexture);
                    GUI.color = Color.white;
                }


                if ( indent == INDENT_START && string.IsNullOrEmpty(search) ) {
                    EditorUtils.Separator();
                }
            }
        }

        ///<summary>Ping element. User hover.</summary>
        void PingElement(HierarchyTree.Element e) {
            var element = e.GetFirstParentReferenceOfType<IGraphElement>();
            EditorApplication.delayCall += () => GraphEditor.PingElement(element);
        }

        ///<summary>Focus element. This also Pings it. User click.</summary>
        void FocusElement(HierarchyTree.Element e) {
            var element = e.GetFirstParentReferenceOfType<IGraphElement>();
            EditorApplication.delayCall += () => GraphEditor.FocusElement(element, true);
        }
    }
}

#endif