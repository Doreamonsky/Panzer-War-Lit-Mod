#if UNITY_EDITOR

using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEditor;
using UnityEngine;

namespace NodeCanvas.Editor
{

    //Panels
    partial class GraphEditor
    {

        private const float PANELS_Y = 2;
        private static float inspectorPanelHeight;
        private static float blackboardPanelHeight;
        private static bool isResizingInspectorPanel;
        private static bool isResizingBlackboardPanel;
        private static Vector2 inspectorPanelScrollPos;
        private static Vector2 blackboardPanelScrollPos;
        private static bool inspectorPanelNeedsScroller;
        private static bool blackboardPanelNeedsScroller;

        private static UnityEditor.AnimatedValues.AnimFloat inspectorPanelAnimFloat;
        private static System.WeakReference<IGraphElement> _inspectorPanelPreviousSelectionRef;
        private static IGraphElement inspectorPanelPreviousSelection {
            get { _inspectorPanelPreviousSelectionRef.TryGetTarget(out IGraphElement element); return element; }
            set { _inspectorPanelPreviousSelectionRef.SetTarget(value); }
        }

        [InitializeOnLoadMethod]
        static void Init_Panels() {
            inspectorPanelAnimFloat = new UnityEditor.AnimatedValues.AnimFloat(0);
            _inspectorPanelPreviousSelectionRef = new System.WeakReference<IGraphElement>(null);
            GraphEditorUtility.onActiveElementChanged -= OnActiveElementChanged;
            GraphEditorUtility.onActiveElementChanged += OnActiveElementChanged;
        }

        static void OnActiveElementChanged(IGraphElement element) {
            if ( Prefs.animatePanels ) {
                //cache previous selection so that animate out has something to show
                if ( element != null ) { inspectorPanelPreviousSelection = element; }
                inspectorPanelAnimFloat.speed = 2.25f;
                inspectorPanelAnimFloat.target = element == null ? 0 : 1;
                return;
            }
            inspectorPanelPreviousSelection = null;
            inspectorPanelAnimFloat.value = element == null ? 0 : 1;
        }

        //This is called outside of windows
        static void ShowPanels(Graph graph, Vector2 canvasMousePos) {
            ShowGraphCommentsGUI(graph, canvasMousePos);
            var panel1 = ShowInspectorGUIPanel(graph, canvasMousePos).ExpandBy(14);
            var panel2 = ShowBlackboardGUIPanel(graph, canvasMousePos).ExpandBy(14);
            GraphEditorUtility.allowClick = !panel1.Contains(e.mousePosition) && !panel2.Contains(e.mousePosition);
        }

        //Show the comments window
        static void ShowGraphCommentsGUI(Graph graph, Vector2 canvasMousePos) {
            if ( Prefs.showComments && !string.IsNullOrEmpty(graph.comments) ) {
                GUI.backgroundColor = Color.white.WithAlpha(0.3f);
                var content = EditorUtils.GetTempContent(graph.comments);
                var calcHeight = StyleSheet.commentsBox.CalcHeight(content, Prefs.inspectorPanelWidth);
                var rect = new Rect(canvasRect.xMin + 2, canvasRect.yMax - calcHeight - 2, Prefs.inspectorPanelWidth, calcHeight);
                GUI.Box(rect, graph.comments, StyleSheet.commentsBox);
                GUI.backgroundColor = Color.white;
            }
        }

        //node, connection inspector panel
        static Rect ShowInspectorGUIPanel(Graph graph, Vector2 canvasMousePos) {
            var inspectorPanel = default(Rect);

            if ( Prefs.useExternalInspector ) {
                return inspectorPanel;
            }

            if ( inspectorPanelAnimFloat.isAnimating ) { willRepaint = true; }
            if ( inspectorPanelAnimFloat.value == 0 && !inspectorPanelAnimFloat.isAnimating ) {
                inspectorPanelPreviousSelection = null;
                inspectorPanelHeight = 0;
                return inspectorPanel;
            }

            var inspectedElement = Prefs.animatePanels ? inspectorPanelPreviousSelection : GraphEditorUtility.activeElement;
            if ( inspectedElement == null ) {
                inspectorPanelHeight = 0;
                return inspectorPanel;
            }

            GUI.BeginClip(canvasRect);

            var scrollWidth = inspectorPanelNeedsScroller ? 16 : 0;
            var posX = 5;
            var posY = PANELS_Y;
            var headerHeight = 30;

            inspectorPanel.x = Mathf.Lerp(-Prefs.inspectorPanelWidth - scrollWidth, posX, inspectorPanelAnimFloat.value);
            inspectorPanel.y = posY;
            inspectorPanel.width = Prefs.inspectorPanelWidth;
            inspectorPanel.height = inspectorPanelHeight;

            var groupRect = inspectorPanel.ExpandBy(0, 0, scrollWidth, 0);
            GUI.Box(groupRect, string.Empty, StyleSheet.windowShadow);
            //remove potential new lines. we want title to be shown as one liner here
            var displayName = !string.IsNullOrEmpty(inspectedElement.name) ? inspectedElement.name.Replace("\n", "") : inspectedElement.name;
            GUI.Box(groupRect, displayName, StyleSheet.editorPanel);

            var headerRect = new Rect(inspectorPanel.x, inspectorPanel.y, inspectorPanel.width, 30);
            var resizeRect = Rect.MinMaxRect(inspectorPanel.xMax - 2, inspectorPanel.yMin, inspectorPanel.xMax + 2, inspectorPanel.yMax);
            var popRect = new Rect(0, 0, 16, 16);
            var gearRect = new Rect(0, 0, 16, 16);
            popRect.center = new Vector2(inspectorPanel.xMin + 10, headerRect.center.y);
            gearRect.center = new Vector2(inspectorPanel.xMax - 16, headerRect.center.y);

            EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeHorizontal);
            EditorGUIUtility.AddCursorRect(popRect, MouseCursor.Link);
            EditorGUIUtility.AddCursorRect(gearRect, MouseCursor.Link);

            if ( e.type == EventType.MouseDown && resizeRect.Contains(e.mousePosition) ) { isResizingInspectorPanel = true; e.Use(); }
            if ( isResizingInspectorPanel && e.type == EventType.Layout ) { Prefs.inspectorPanelWidth += e.delta.x; }
            if ( e.rawType == EventType.MouseUp ) { isResizingInspectorPanel = false; }

            GUI.color = GUI.color.WithAlpha(0.5f);
            if ( GUI.Button(popRect, EditorUtils.GetTempContent(Icons.minMaxIcon, "Open External Inspector"), GUIStyle.none) ) {
                ExternalInspectorWindow.ShowWindow();
            }
            GUI.color = Color.white;
            if ( inspectedElement is Node ) {
                if ( GUI.Button(gearRect, EditorUtils.GetTempContent(Icons.gearPopupIcon, "Context Menu"), GUIStyle.none) ) {
                    Node.GetNodeMenu_Single(inspectedElement as Node).ShowAsContext();
                }
            }

            EditorGUIUtility.AddCursorRect(headerRect, MouseCursor.Link);
            if ( GUI.Button(headerRect, string.Empty, GUIStyle.none) ) {
                Prefs.showNodePanel = !Prefs.showNodePanel;
            }

            GUI.BeginGroup(groupRect);
            if ( Prefs.showNodePanel ) {

                var contentRect = Rect.MinMaxRect(2, headerHeight, inspectorPanel.width - 2, inspectorPanel.height);
                var position = Rect.MinMaxRect(0, headerHeight, inspectorPanel.width + scrollWidth, Mathf.Min(inspectorPanel.height, canvasRect.height - posX));
                var viewRect = Rect.MinMaxRect(0, headerHeight, inspectorPanel.width, inspectorPanel.height);
                inspectorPanelNeedsScroller = position.height < viewRect.height;
                inspectorPanelScrollPos = GUI.BeginScrollView(position, inspectorPanelScrollPos, viewRect, false, false);
                GUILayout.BeginArea(contentRect);

                if ( inspectedElement is Node ) { Node.ShowNodeInspectorGUI((Node)inspectedElement); }
                if ( inspectedElement is Connection ) { Connection.ShowConnectionInspectorGUI((Connection)inspectedElement); }

                EditorUtils.EndOfInspector();
                if ( e.type == EventType.Repaint ) {
                    inspectorPanelHeight = GUILayoutUtility.GetLastRect().yMax + headerHeight + 5;
                }

                GUILayout.EndArea();
                GUI.EndScrollView();

            } else {

                inspectorPanelHeight = 55;
                var contentRect = Rect.MinMaxRect(0, headerHeight, inspectorPanel.width, inspectorPanel.height);
                EditorGUIUtility.AddCursorRect(contentRect, MouseCursor.Link);
                if ( GUI.Button(contentRect, "<b>...</b>", Styles.centerLabel) ) {
                    Prefs.showNodePanel = true;
                }
            }

            GUI.EndGroup();
            GUI.EndClip();

            inspectorPanel.x += canvasRect.x;
            inspectorPanel.y += canvasRect.y;

            return inspectorPanel;
        }


        //blackboard inspector panel
        static Rect ShowBlackboardGUIPanel(Graph graph, Vector2 canvasMousePos) {
            var blackboardPanel = default(Rect);
            if ( graph.blackboard == null ) {
                blackboardPanelHeight = 0;
                return blackboardPanel;
            }

            GUI.BeginClip(canvasRect);

            var scrollWidth = blackboardPanelNeedsScroller ? 16 : 0;
            var posX = scrollWidth + 5;
            var posY = PANELS_Y;
            var headerHeight = 30;

            blackboardPanel.x = canvasRect.xMax - Prefs.blackboardPanelWidth - posX - canvasRect.x;
            blackboardPanel.y = posY;
            blackboardPanel.width = Prefs.blackboardPanelWidth;
            blackboardPanel.height = blackboardPanelHeight;

            var resizeRect = Rect.MinMaxRect(blackboardPanel.xMin - 2, blackboardPanel.yMin, blackboardPanel.xMin + 2, blackboardPanel.yMax);
            EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeHorizontal);
            if ( e.type == EventType.MouseDown && resizeRect.Contains(e.mousePosition) ) { isResizingBlackboardPanel = true; e.Use(); }
            if ( isResizingBlackboardPanel && e.type == EventType.Layout ) { Prefs.blackboardPanelWidth -= e.delta.x; }
            if ( e.rawType == EventType.MouseUp ) { isResizingBlackboardPanel = false; }

            var headerRect = new Rect(blackboardPanel.x, blackboardPanel.y, blackboardPanel.width, 30);
            EditorGUIUtility.AddCursorRect(headerRect, MouseCursor.Link);
            if ( GUI.Button(headerRect, string.Empty, GUIStyle.none) ) {
                Prefs.showBlackboard = !Prefs.showBlackboard;
            }

            var groupRect = blackboardPanel.ExpandBy(0, 0, scrollWidth, 0);
            GUI.Box(groupRect, string.Empty, StyleSheet.windowShadow);
            GUI.Box(groupRect, "Blackboard Variables", StyleSheet.editorPanel);

            GUI.BeginGroup(groupRect);
            if ( Prefs.showBlackboard ) {

                var contentRect = Rect.MinMaxRect(2, headerHeight, blackboardPanel.width - 2, blackboardPanel.height);
                var position = Rect.MinMaxRect(0, headerHeight, blackboardPanel.width + scrollWidth, Mathf.Min(blackboardPanel.height, canvasRect.height - posX) + scrollWidth);
                var viewRect = Rect.MinMaxRect(0, headerHeight, blackboardPanel.width, blackboardPanel.height);
                blackboardPanelNeedsScroller = position.height < viewRect.height;
                blackboardPanelScrollPos = GUI.BeginScrollView(position, blackboardPanelScrollPos, viewRect, false, false);
                GUILayout.BeginArea(contentRect);

                BlackboardEditor.ShowVariables(graph.blackboard, graph);
                EditorUtils.EndOfInspector();
                if ( e.type == EventType.Repaint ) {
                    blackboardPanelHeight = GUILayoutUtility.GetLastRect().yMax + headerHeight + 5;
                }

                GUILayout.EndArea();
                GUI.EndScrollView();

            } else {

                blackboardPanelHeight = 55;
                var contentRect = Rect.MinMaxRect(0, headerHeight, blackboardPanel.width, blackboardPanel.height);
                EditorGUIUtility.AddCursorRect(contentRect, MouseCursor.Link);
                if ( GUI.Button(contentRect, "<b>...</b>", Styles.centerLabel) ) {
                    Prefs.showBlackboard = true;
                }
            }

            GUI.EndGroup();
            GUI.EndClip();

            blackboardPanel.x += canvasRect.x;
            blackboardPanel.y += canvasRect.y;

            if ( graph.canAcceptVariableDrops && BlackboardEditor.pickedVariable != null && BlackboardEditor.pickedVariableBlackboard.IsPartOf(graph.blackboard) ) {
                GUI.Label(new Rect(e.mousePosition.x + 15, e.mousePosition.y, 100, 18), "Drop Variable", StyleSheet.labelOnCanvas);
                if ( e.type == EventType.MouseUp && !blackboardPanel.Contains(e.mousePosition) ) {
                    graph.CallbackOnVariableDropInGraph(BlackboardEditor.pickedVariableBlackboard, BlackboardEditor.pickedVariable, canvasMousePos);
                    BlackboardEditor.ResetPick();
                }
            }

            return blackboardPanel;
        }

    }
}

#endif